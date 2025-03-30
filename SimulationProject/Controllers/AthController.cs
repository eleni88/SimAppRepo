using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimulationProject.DTO;
using SimulationProject.Services;

namespace SimulationProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AthController : ControllerBase
    {
        private readonly IUsersService _usersService;
        private readonly AthService _athService;
        public AthController(IUsersService usersService, AthService athService)
        {
            _usersService = usersService;
            _athService = athService;
        }

        //----------- Register -------------

        // POST /api/ath/register
        [HttpPost("register")]
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
            if (!_usersService.PasswordValid(registerForm.Password))
            {
                return BadRequest("Invalid password. Password must be at least 10 characters long, contain at least one uppercase letter, one lowercase letter, one number, and one special character.");
            }

            var user = await _athService.RegisterUserAsync(registerForm);
            if (user is null)
            {
                return BadRequest("User not found!");
            }

            return Ok(user);
        }

        //----------- Login -------------

        //POST /api/ath/login
        [HttpPost("login")]
        public async Task<ActionResult<TokenDTo>> LoginUser([FromBody] LoginForm loginform)
        {
            var result = await _athService.LoginUserAsync(loginform);
            if (result is null)
            {
                return BadRequest("Invalid username or password");
            }

            // Store the Access JWT in a cookie
            var cookieOptions = _athService.GetCookieOptions();
            Response.Cookies.Append("jwtCookie", result.AccessToken, cookieOptions);

            return Ok(result);
        }

        //----------- Logout -------------
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwtCookie");
            return Ok(new { message = "Logged out successfully" });
        }

        [Authorize]
        [HttpGet]
        public IActionResult AuthenticatedOnlyEndPoint()
        {
            return Ok("You are Authenticated!");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminEndPoint()
        {
            return Ok("You are Admin!");
        }

    }
}
