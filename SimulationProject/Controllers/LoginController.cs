using Microsoft.AspNetCore.Mvc;
using SimulationProject.Services;
using SimulationProject.DTO;

namespace SimulationProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly UsersProfileService _usersProfileService;

        public LoginController(UsersProfileService usersProfileService)
        {
            _usersProfileService = usersProfileService;
        }
        //POST /api/login
        [HttpPost]
        public async Task<IActionResult> LoginUser([FromBody] LoginForm loginform)
        {
            var token = await _usersProfileService.LoginUserAsync(loginform);
            if (token is null){
                return BadRequest("Invalid username or password");   
            }

            // Store the JWT in a cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,                       // Make sure the cookie is not accessible via JavaScript (for security)
                Secure = true,                         // Ensures the cookie is only sent over HTTPS
                SameSite = SameSiteMode.Strict,        // Prevents cross-site request forgery (CSRF)
                Expires = DateTime.UtcNow.AddDays(1)   // Set the expiration of the cookie
            };
            Response.Cookies.Append("jwtCookie", token, cookieOptions);
            
            return Ok(token);
        }
    }

}
