using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SimulationProject.DTO;
using SimulationProject.Models;
using SimulationProject.Services;

namespace SimulationProject.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUsersService _usersService;
        private readonly UsersProfileService _usersProfileService;
        private readonly LinkGenerator _linkGenerator;
        public UsersController(IUsersService usersService, UsersProfileService usersProfileService, LinkGenerator linkGenerator)
        {
            _usersService = usersService;
            _usersProfileService = usersProfileService;
            _linkGenerator = linkGenerator;
        }
        // GET /api/users
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var user = await _usersService.GetAllUsersAsync();
            if ((user == null) || (!user.Any()))
            {
                return Ok(new List<object>());
            }
            return Ok(user);
        }

        // GET /api/users/{Userid}
        [HttpGet("{Userid}")]
        public async Task<IActionResult> GetUser(int Userid)
        {
            var user = await _usersService.GetUserByIdAsync(Userid);
            if (user == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "User Not Found",
                    Detail = $"No User found with ID {Userid}.",
                    Status = 404
                });
            }

            var links = new
            {
                self = new { href = _linkGenerator.GetPathByAction(HttpContext, nameof(GetUser), values: new { Userid }), method = "GET" },
                update = new { href = _linkGenerator.GetPathByAction(HttpContext, nameof(UpdateUser), values: new { Userid }), method = "PUT" },
                delete = new { href = _linkGenerator.GetPathByAction(HttpContext, nameof(DeleteUser), values: new { Userid }), method = "DELETE" }
            };

            return Ok(user);
        }

        // POST /api/users/create
        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest();
            }
            if (!(_usersService.UserNameExists(user.Username) && _usersService.UserEmailExists(user.Email)))
            {
                await _usersService.CreateUserAsync(user);
            }
            else
            {
                if (_usersService.UserNameExists(user.Username))
                {
                    ModelState.AddModelError("Username", "The username is used by another user");
                }
                if (_usersService.UserEmailExists(user.Email))
                {
                    ModelState.AddModelError("Useremail", "The email is used by another user");
                }
            }
            return CreatedAtAction(nameof(GetUser), new { Userid = user.Userid }, user);
        }

        // PUT /api/users/{Userid}
        [HttpPost("{Userid}")]
        public async Task<IActionResult> UpdateUser(int Userid, [FromBody]UserDto userDto)
        {
            var user = await _usersService.GetUserByIdAsync(Userid);
            if (user == null)
            {
                return BadRequest("User not found.");
            }
            if (_usersService.UserNameExists(userDto.UserName))
            {
                return BadRequest("The username is used by another user");
            }
            if (_usersService.UserEmailExists(userDto.Email))
            {
                return BadRequest("The email is used by another user");
            }
            await _usersService.PutUserAsync(user);

            return NoContent();
        }

        // DELETE /api/users/{Userid}
        [HttpDelete("{Userid}")]
        public async Task<IActionResult> DeleteUser(int Userid)
        {
            var user = await _usersService.GetUserByIdAsync(Userid);
            if (user == null)
            {
                return NotFound();
            }
            await _usersService.DeleteUserAsync(user);
            return NoContent();
        }

        //------ User Profile --------------
        // PUT /api/profile
        [HttpPost("profile")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UserDto userDto)
        {
            //extract user from token
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                return BadRequest("Invalid user.");
            }
            var userId = Int32.Parse(userIdStr);
            var user = await _usersService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return BadRequest("User not found.");
            }
            if (_usersService.UserNameExists(userDto.UserName))
            {
                return BadRequest("The username is used by another user");
            }
            if (_usersService.UserEmailExists(userDto.Email))
            {
                return BadRequest("The email is used by another user");
            }

            int rowsAfected = await _usersProfileService.PutUserAsync(user, userDto);
            if (rowsAfected > 0)
            {
                return Ok("User updated successfully.");
            }

            return NoContent();
        }

        // DELETE /api/users
        [HttpDelete]
        public async Task<IActionResult> DeleteUserProfile([FromBody]UserDto userDto)
        {
            //extract user from token
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                return BadRequest("Invalid user.");
            }
            var userId = Int32.Parse(userIdStr);
            var user = await _usersService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            await _usersService.DeleteUserAsync(user);
            return NoContent();
        }
    }
}
