using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimulationProject.Services;
using System.Security.Claims;
using SimulationProject.DTO.UserDTOs;

namespace SimulationProject.Controllers
{
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
        [Authorize]
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
            
            string newpass = _usersService.GetUserNewPassword(PasswordUpdate.NewPassword, user.Username, user, PasswordUpdate.OldPassword);
            if (string.IsNullOrEmpty(newpass))
            {
                return BadRequest(new { message = "Wrong password." });
            }
            
            await _usersService.UpdateUserPasswordAsync(newpass, user);
            return Ok(new { message = "Password updated successfully." });
        }

        //PUT /api/passwordchange/generate
        [HttpPut("generate")]
        public async Task<IActionResult> GenerateTempCode([FromBody] TempcodeRequestDTO tempcodeRequest)
        {
            string tempCode = await _usersService.GenerateAndSaveTempCode(tempcodeRequest.username);
            if (string.IsNullOrEmpty(tempCode)){
                return BadRequest();
            } 
            return Ok();
        }

        //PUT /api/passwordchange/reset
        [HttpPut("reset")]
        public async Task<IActionResult> ResetUserPassword([FromBody] PasswordReset PasswordResset)
        {
            var user = await _usersService.GetUserByNameAsync(PasswordResset.UserName);
            if (user == null)
            {
                return NotFound(new { message = "User not Found." });
            }
            if ((user.Emailtimestamp != null) && ((DateTime.UtcNow - user.Emailtimestamp) > TimeSpan.FromMinutes(10)))
            {
                await _usersService.SetUserInActive(user);
                return BadRequest(new { message = "Temporary code has expired. Your account has been disabled. Please contact the admin." });
            }

            string newpass = _usersService.GetUserNewPassword(PasswordResset.NewPassword, PasswordResset.UserName, user, "", PasswordResset.TempPassword);

            if (string.IsNullOrEmpty(newpass))
            {
                return BadRequest(new { message = "Wrong username or password." });
            }

            await _usersService.UpdateUserPasswordAsync(newpass, user);
            await _usersService.ResetTempPassAndTimeStamp(user);
            return Ok(new { message = "Password updated successfully." });
        }
    }
}
