using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimulationProject.DTO;
using SimulationProject.Services;
using SimulationProject.Models;

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
        //POST /api/passwordchange/userid

        [HttpPost("{Userid}")]
        public async Task<IActionResult> UpdateUserPassword([FromBody] PasswordUpdate PasswordUpdate, User user)
        {
            string newpass = _usersService.GetUserNewPassword(PasswordUpdate, user.Password, user.Username);
            if (newpass == "1")
            {
                return BadRequest("Wrong credentials.");
            }
            if (newpass == "2")
            {
                return BadRequest("Invalid password. Password must be at least 10 characters long, contain at least one uppercase letter, one lowercase letter, one number, and one special character.");
            }
            if (newpass == "3")
            {
                return BadRequest("Password don't match.");
            }
              
            await _usersService.UpdateUserPasswordAsync(user);
            return Ok("Password updated successfully.");
        }
    }
}
