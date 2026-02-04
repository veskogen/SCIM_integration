using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScimProvisioningApp.Models;
using ScimProvisioningApp.Services;
using System.Linq;

namespace ScimProvisioningApp.Controllers
{
[ApiController]
[Route("scim/v2")]
[AllowAnonymous] // Middleware handles the security instead
    public class ScimController : ControllerBase
    {
        private readonly ScimService _scimService;
        private readonly ILogger<ScimController> _logger;

        public ScimController(ScimService scimService, ILogger<ScimController> logger)
        {
            _scimService = scimService;
            _logger = logger;
        }


// Entra ID calls this to see if the server is SCIM-compliant
[HttpGet("ServiceProviderConfig")]
[AllowAnonymous] // Entra ID often checks this without a token first
public IActionResult GetServiceProviderConfig()
{
    return Ok(new {
        schemas = new[] { "urn:ietf:params:scim:schemas:core:2.0:ServiceProviderConfig" },
        patch = new { supported = true }, // Set to true if you can handle PATCH
        bulk = new { supported = false, maxOperations = 0, maxPayloadSize = 0 },
        filter = new { supported = true, maxResults = 200 },
        changePassword = new { supported = false },
        sort = new { supported = false },
        etag = new { supported = false },
        authenticationSchemes = new[] { 
            new { 
                name = "OAuth Bearer Token",
                description = "Authentication via OAuth Bearer Token",
                specUri = "http://tools.ietf.org",
                type = "oauthbearertoken", 
                primary = true 
            } 
        }
    });
}

// Entra ID calls this to understand your user/group structure - not sure if needed
// [HttpGet("Schemas")]
// [AllowAnonymous]
// public IActionResult GetSchemas()
// {
//     return Ok(new {
//         schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:ListResponse" },
//         totalResults = 0,
//         Resources = new List<object>()
//     });
// }


// GET /Users: List users
[HttpGet("Users")]
public IActionResult GetUsers([FromQuery] string? filter = null)
{
    var users = _scimService.GetAllUsers().Cast<UserModel>().ToList();

    if (!string.IsNullOrEmpty(filter))
    {
        // Safer parsing for: userName eq "user@example.com"
        var parts = filter.Split('"');
        if (parts.Length >= 2)
        {
            var filterValue = parts[1];
            users = users.Where(u => u.UserName.Equals(filterValue, StringComparison.OrdinalIgnoreCase)).ToList();
            _logger.LogInformation($"Filtering users by userName: {filterValue}");
        }
    }

    return Ok(new
    {
        schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:ListResponse" },
        totalResults = users.Count,
        startIndex = 1,
        itemsPerPage = users.Count,
        resources = users.Select(u => new {
            id = u.Id,
            userName = u.UserName,
            name = new { givenName = u.FirstName, familyName = u.LastName },
            emails = new[] { new { value = u.Email, type = "work", primary = true } },
            active = u.Active
        })
    });
}

        // GET /Users/{id}: Retrieve a single user
        [HttpGet("Users/{id}")]
        public IActionResult GetUser(string id)
        {
            var user = _scimService.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        // POST /Users: Create a new user
        [HttpPost("Users")]
public IActionResult CreateUser([FromBody] ScimUserCreateModel scimUser)
{
    var userModel = new UserModel
    (
        Id: Guid.NewGuid().ToString(),
        UserName: scimUser.UserName,
        FirstName: scimUser.Name?.GivenName ?? "",
        LastName: scimUser.Name?.FamilyName ?? "",
        Email: scimUser.Emails?.FirstOrDefault()?.Value ?? "",
        Active: scimUser.Active
    );

    var newUser = _scimService.CreateUser(userModel);
    _logger.LogInformation($"Created new user with ID: {newUser.Id}");

    // Entra ID needs the SCIM schema back, not just the raw UserModel
    return Created($"/scim/v2/Users/{newUser.Id}", new {
        schemas = new[] { "urn:ietf:params:scim:schemas:core:2.0:User" },
        id = newUser.Id,
        userName = newUser.UserName,
        name = new { givenName = newUser.FirstName, familyName = newUser.LastName },
        emails = new[] { new { value = newUser.Email, type = "work", primary = true } },
        active = newUser.Active
    });
}

        // PUT /Users/{id}: Update a user
        [HttpPut("Users/{id}")]
        public IActionResult UpdateUser(string id, [FromBody] UserModel userModel)
        {
            var updatedUser = _scimService.UpdateUser(id, userModel);
            if (updatedUser == null)
            {
                return NotFound();
            }
            return Ok(updatedUser);
        }

        // DELETE /Users/{id}: Delete a user
        [HttpDelete("Users/{id}")]
        public IActionResult DeleteUser(string id)
        {
            _logger.LogInformation($"Attempting to delete user with ID: {id}");
            var result = _scimService.DeleteUser(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
