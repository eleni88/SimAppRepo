using System.Security.Claims;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimulationProject.DTO.UserDTOs;
using SimulationProject.Models;
using SimulationProject.Services;

namespace SimulationProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUsersService _usersService;
        private readonly ILinkService<UserDto> _linkService;

        public UsersController(IUsersService usersService, ILinkService<UserDto> linkService)
        {
            _usersService = usersService;
            _linkService = linkService;
        }
        // GET /api/users
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {   
            string baseUri = $"{Request.Scheme}://{Request.Host}";
            var users = await _usersService.GetAllUsersAsync();
            if ((users == null) || (!users.Any()))
            {
                return Ok(new List<object>());
            }

            var userdtos = users.Select(user => user.Adapt<UserDto>());
            var UsersWithLinks = _linkService.AddLinksToList(userdtos, baseUri);

            return Ok(UsersWithLinks);
        }

        // GET /api/users/{Userid}
        [Authorize(Roles = "Admin")]
        [HttpGet("{Userid}")]
        public async Task<IActionResult> GetUser(int Userid)
        {
            string baseUri = $"{Request.Scheme}://{Request.Host}";
            var user = await _usersService.GetUserByIdAsync(Userid);
            if (user is null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "User Not Found",
                    Detail = $"No User found with ID {Userid}.",
                    Status = 404
                });
            }

            var userdto = user.Adapt<UserDto>();
            
            var UserWithlinks = _linkService.AddLinksForUser(userdto, baseUri);

            return Ok(UserWithlinks);
        }

        // POST /api/users/create
        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO userdtto)
        {
            var user = userdtto.Adapt<User>();
            if (user == null)
            {
                return BadRequest();
            }
            if (!(_usersService.UserNameExists(-1, user.Username) && _usersService.UserEmailExists(-1, user.Email)))
            {
                user.Role = _usersService.FindUserRole(Convert.ToInt32(userdtto.Admin));
                await _usersService.CreateUserAsync(user);
            }
            else
            {
                if (_usersService.UserNameExists(-1 ,user.Username))
                {
                    return BadRequest(new { message = "The username is used by another user" });
                }
                if (_usersService.UserEmailExists(-1, user.Email))
                {
                    return BadRequest(new { message = "The email is used by another user" });
                }
            }
            return CreatedAtAction(nameof(GetUser), new { Userid = user.Userid }, user);
        }

        // PUT /api/users/{Userid}
        [Authorize(Roles = "Admin")]
        [HttpPost("{Userid}")]
        public async Task<IActionResult> UpdateUser(int Userid, [FromBody] UpdateUserDTO userDto)
        {
            var user = await _usersService.GetUserByIdAsync(Userid);
            if (user == null)
            {
                return BadRequest(new { message = "User not found." });
            }
            if (_usersService.UserNameExists(Userid, userDto.Username))
            {
                return BadRequest(new { message = "The username is used by another user" });
            }
            if (_usersService.UserEmailExists(Userid, userDto.Email))
            {
                return BadRequest(new { message = "The email is used by another user" });
            }

            userDto.Adapt<User>();
            await _usersService.PutUserAsync();

            return NoContent();
        }

        // DELETE /api/users/{Userid}
        [Authorize(Roles = "Admin")]
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

        //-------------------------------- User Profile --------------------------------------
        //GET /api/profile
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> ViewUserProfile()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                return BadRequest(new { message = "Invalid user" });
            }
            var userId = Int32.Parse(userIdStr);
            var user = await _usersService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return BadRequest(new { message = "User not found" });
            }
            var userDto = user.Adapt<UserDto>();
            return Ok(userDto);
        }

        // PUT /api/profile
        [Authorize]
        [HttpPost("profile")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserProfileDTO userDto)
        {
            //extract user from token
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                return BadRequest(new { message = "Invalid user" });
            }
            var userId = Int32.Parse(userIdStr);
            var user = await _usersService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return BadRequest(new { message = "User not found"});
            }
            if (_usersService.UserNameExists(userId ,userDto.Username))
            {
                return BadRequest(new { message = "The username is used by another user" });
            }
            if (_usersService.UserEmailExists(userId, userDto.Email))
            {
                return BadRequest(new { message = "The email is used by another user" });
            }

            int rowsAfected = await _usersService.PutUserProfileAsync(user, userDto);
            if (rowsAfected > 0)
            {
                return Ok(new { message = "User updated successfully" });
            }

            return NoContent();
        }

        // DELETE /api/users
        [Authorize(Roles = "User")]
        [HttpDelete("profile")]
        public async Task<IActionResult> DeleteUserProfile()
        {
            //extract user from token
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                return BadRequest(new { message = "Invalid user" });
            }
            var userId = Int32.Parse(userIdStr);
            var user = await _usersService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return BadRequest(new { message = "User not found" });
            }
            await _usersService.DeleteUserAsync(user);
            return NoContent();
        }

        [Authorize]
        [HttpPost("questions")]
        public async Task<IActionResult> ShowSecurityQuestions([FromBody] SecurityQuestionsAndAnswersDTO QuestionsDto)
        {
            //extract user from token
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                return BadRequest(new { message = "Invalid user" });
            }
            var userId = Int32.Parse(userIdStr);
            var user = await _usersService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return BadRequest(new { message = "User not found" });
            }
            if (_usersService.SecurityAnswer(user, QuestionsDto)){
            }
            return Ok(new { message = "Validation success." }); 
        }
    }
}
