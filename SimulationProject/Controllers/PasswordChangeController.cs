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
        //PUT /api/passwordchange/update
        [HttpPut("update")]
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
            
            string newpass = _usersService.GetUserNewPassword(PasswordUpdate.NewPassword, PasswordUpdate.OldPassword, PasswordUpdate.UserName, user);
            if (string.IsNullOrEmpty(newpass))
            {
                return BadRequest(new { message = "Wrong username or password." });
            }
            
            await _usersService.UpdateUserPasswordAsync(newpass, user);
            return Ok(new { message = "Password updated successfully." });
        }

        //PUT /api/passwordchange/reset
        [HttpPut("reset")]
        public async Task<IActionResult> ResetUserPassword([FromBody] PasswordReset PasswordResset)
        {
            string tempCode = _usersService.GenerateAndSaveTempCode(PasswordResset.UserName);

            return Ok();
        }
    }
}
