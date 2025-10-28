using Microsoft.AspNetCore.Mvc;
using ScimProvisioningApp.Models;
using ScimProvisioningApp.Services;

namespace ScimProvisioningApp.Controllers
{
    [ApiController]
    [Route("scim/v2")]
    public class ScimController : ControllerBase
    {
        private readonly ScimService _scimService;

        public ScimController(ScimService scimService)
        {
            _scimService = scimService;
        }

        // GET /Users: List users
        [HttpGet("Users")]
        public IActionResult GetUsers()
        {
            return Ok(_scimService.GetAllUsers());
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
            // Map the nested SCIM payload to your flat UserModel
            var userModel = new UserModel
            (
                Id: Guid.NewGuid().ToString(), // ID will be set by the service
                UserName: scimUser.UserName,
                FirstName: scimUser.Name?.GivenName,
                LastName: scimUser.Name?.FamilyName,
                Email: scimUser.Emails?.FirstOrDefault()?.Value,
                Active: scimUser.Active
            );

            var newUser = _scimService.CreateUser(userModel);
            return CreatedAtAction(nameof(GetUser), new { id = newUser.Id }, newUser);
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
            var result = _scimService.DeleteUser(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
        
        // GET /: Base path for Entra ID connection test
        [HttpGet]
        public IActionResult GetBase()
        {
            return Ok();
        }
    }
}
