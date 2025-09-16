using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimulationProject.DTO.UserDTOs;
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
            var user = await _athService.RegisterUserAsync(registerForm);        
            if (user is null)
            {
                return BadRequest(new { message = "Registration failed." });
            }

            return Ok();
        }

        //----------- Login -------------

        //POST /api/ath/login
        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] LoginForm loginform)
        {
            var result = await _athService.LoginUserAsync(loginform);
            bool isactive = await _athService.CheckUserActiveAsync(loginform);
            
            if ((result is null) && (isactive))
            {
                return BadRequest(new { message = "Invalid username or password" });
            }

            if (!isactive)
            {
                return BadRequest(new { message = "Your account has been disabled. Please contact the admin." });
            }

            // Store the Access JWT in partinioned cookie
            var cookieOptions = _athService.SetPartitionedCookie("jwtCookie", result.AccessToken, 900);
            Response.Headers.Append("Set-Cookie", cookieOptions);

            // Store the Refresh token in partinioned cookie
            var cookieRefreshOptions = _athService.SetPartitionedCookie("RefreshTokenCookie", result.RefreshToken, 604800);
            Response.Headers.Append("Set-Cookie", cookieRefreshOptions);

            return Ok(new { message = "Login successful" });
        }

        //----------- Logout -------------
        //POST /api/ath/logout
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            //extract user from token
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                return BadRequest(new { message = "Invalid user" });
            }
            var userId = Int32.Parse(userIdStr);
            var user = await _usersService.GetUserByIdAsync(userId);
            if ((user != null) && (user.Refreshtoken != null))
            {
                if (await _athService.RemoveRefreshTokenAsync(user.Userid, user.Refreshtoken))
                {
                    // Delete JWT
                    var cookieOptions = _athService.SetPartitionedCookie("jwtCookie", "", 0);
                    Response.Headers.Append("Set-Cookie", cookieOptions);
                    // Delete Refresh Token cookie
                    var cookieRefreshOptions = _athService.SetPartitionedCookie("RefreshTokenCookie", "", 0);
                    Response.Headers.Append("Set-Cookie", cookieRefreshOptions);


                    return Ok(new { message = "Logged out successfully" });
                }
                else
                    return BadRequest(new { message = "Logout failed" });
            }
            return Ok();
        }

        //----------Refresh Token -----------
        //POST /api/ath/refreshtoken
        [HttpPost("refreshtoken")]
        public async Task<ActionResult<TokenDTo>> RefreshToken()
        {
            var request = Request.Cookies["RefreshTokenCookie"];

            if (string.IsNullOrEmpty(request))
                return Unauthorized();

            var user = await _usersService.GetUserByRefreshTokenAsync(request);
            var result = await _athService.RefreshTokenAsync(user, request);
            if ((result == null) || (result.AccessToken == null) || (result.RefreshToken == null))
            {
                return Unauthorized(new { message = "Invalid refresh token" });
            }

            // Store the Access JWT in partinioned cookie
            var cookieOptions = _athService.SetPartitionedCookie("jwtCookie", result.AccessToken, 900);
            Response.Headers.Append("Set-Cookie", cookieOptions);

            // Store the Refresh token in partinioned cookie
            var cookieRefreshOptions = _athService.SetPartitionedCookie("RefreshTokenCookie", result.RefreshToken, 604800);
            Response.Headers.Append("Set-Cookie", cookieRefreshOptions);

            return Ok(result);
        }


        //------------- test ----------------
        [Authorize]
        [HttpGet("test-All")]
        public IActionResult AuthenticatedOnlyEndPoint()
        {
            return Ok(new { message = "You are Authenticated!" });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("test-admin-only")]
        public IActionResult AdminEndPoint()
        {
            return Ok(new { message = "You are Admin!" });
        }

    }
}
