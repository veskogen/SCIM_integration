using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScimProvisioningApp.Models;
using ScimProvisioningApp.Services;

namespace ScimProvisioningApp.Controllers
{
    [ApiController]
    [Route("scim/v2")]
    [Authorize]
    public class ScimController : ControllerBase
    {
        private readonly ScimService _scimService;

        public ScimController(ScimService scimService)
        {
            _scimService = scimService;
        }

        [HttpGet("Users")]
        public async Task<IActionResult> GetUsers()
        {
            // Entra ID will not typically perform a GET on all users.
            // This is mainly for testing and discovery.
            return Ok(new { Schemas = new[] { "urn:ietf:params:scim:api:messages:2.0:ListResponse" }, Resources = Array.Empty<ScimUser>() });
        }

        [HttpPost("Users")]
        public async Task<IActionResult> CreateUser([FromBody] ScimUser scimUser)
        {
            try
            {
                var createdUser = await _scimService.CreateUser(scimUser);
                return CreatedAtAction(nameof(CreateUser), new { id = createdUser.Id }, createdUser);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("Users/{id}")]
        public async Task<IActionResult> UpdateUser([FromRoute] string id, [FromBody] ScimUser scimUser)
        {
            try
            {
                var updatedUser = await _scimService.UpdateUser(id, scimUser);
                if (updatedUser == null)
                {
                    return NotFound();
                }
                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("Users/{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] string id)
        {
            try
            {
                await _scimService.DeleteUser(id);
                return NoContent();
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        // Add similar endpoints for Groups if your employee system supports group provisioning.
    }
}
