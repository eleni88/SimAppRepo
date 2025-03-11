using Microsoft.AspNetCore.Mvc;
using SimulationProject.Services;
using SimulationProject.DTO;

namespace SimulationProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly IUsersService _usersService;
        private readonly UsersProfileService _usersProfileService;
        public RegisterController(IUsersService usersService, UsersProfileService usersProfileService)
        {
            _usersService = usersService;
            _usersProfileService = usersProfileService;
        }
        // POST /api/register
        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterForm registerForm)
        {
            if (_usersService.UserNameExists(registerForm.UserName))
            {
                return BadRequest("Username already exists!");
            }
            if (_usersService.UserEmailExists(registerForm.Email))
            {
                return BadRequest("Email already exists.");
            }
            if (_usersService.PasswordValid(registerForm.Password))
            {
                return BadRequest("Invalid password. Password must be at least 10 characters long, contain at least one uppercase letter, one lowercase letter, one number, and one special character.");
            }

            var user = await _usersProfileService.RegisterUserAsync(registerForm);
            if (user is null)
            {
                return BadRequest("User not found!");
            }
            
            return Ok(user);
        }
    }
}
