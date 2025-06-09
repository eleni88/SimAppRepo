using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimulationProject.Services;
using System.Security.Claims;
using SimulationProject.DTO.UserDTOs;

namespace SimulationProject.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordChangeController : ControllerBase
    {
        private readonly IUsersService _usersService;
        public PasswordChangeController(IUsersService usersService)
        {
            _usersService = usersService;
        }
        //POST /api/passwordchange
        [HttpPut]
        public async Task<IActionResult> UpdateUserPassword([FromBody] PasswordUpdate PasswordUpdate)
        {
            //extract user from token
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);        
            if (string.IsNullOrEmpty(userIdStr))
            {
                return BadRequest(new { message = "Invalid user." });
            }
            var userId = Int32.Parse(userIdStr);
            var user = await _usersService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return BadRequest(new { message = "User not found." });
            }
            
            string newpass = _usersService.GetUserNewPassword(PasswordUpdate, user);
            if (newpass == "1")
            {
                return BadRequest(new { message = "Wrong credentials." });
            }
            if (newpass == "2")
            {
                return BadRequest(new { message = "Invalid password. Password must be at least 10 characters long, contain at least one uppercase letter, one lowercase letter, one number, and one special character." });
            }
            if (newpass == "3")
            {
                return BadRequest(new { message = "Password and Confirmation don't match." });
            }
              
            await _usersService.UpdateUserPasswordAsync(newpass, user);
            return Ok(new { message = "Password updated successfully." });
        }
    }
}
