using System.Security.Claims;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimulationProject.DTO.UserDTOs;
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

            //extract user from token
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                return BadRequest(new { message = "Unauthorized user" });
            }
            var userId = Int32.Parse(userIdStr);

            if ((users == null) || (!users.Any()))
            {
                return Ok(new List<object>());
            }

            //get users list without the Admin in it
            var filteredUsers = users
            .Where(u => u.Userid != userId)
            .ToList();

            var userdtos = filteredUsers.Select(user => user.Adapt<UserDto>());
 
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
                return NotFound(new { message = "User Not Found" });
            }

            var userdto = user.Adapt<UserDto>();
            
            var UserWithlinks = _linkService.AddLinks(userdto, baseUri);

            return Ok(UserWithlinks);
        }

        // POST /api/users/create
        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO userdtto)
        {
            
            if (_usersService.UserNameExists(-1, userdtto.Username))
            {
                return BadRequest(new { message = "The username is used by another user" });
            }
            if (_usersService.UserEmailExists(-1, userdtto.Email))
            {
                return BadRequest(new { message = "The email is used by another user" });
            }
            var user = await _usersService.CreateUserAsync(userdtto);
            if (user is null)
            {
                return BadRequest(new { message = "Creation failed." });
            }

            return CreatedAtAction(nameof(GetUser), new { Userid = user.Userid }, user);
        }

        // PUT /api/users/{Userid}
        [Authorize(Roles = "Admin")]
        [HttpPut("{Userid}")]
        public async Task<IActionResult> UpdateUser(int Userid, [FromBody] UpdateUserDTO userDto)
        {
            var user = await _usersService.GetUserByIdAsync(Userid);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }
            if (_usersService.UserNameExists(Userid, userDto.Username))
            {
                return BadRequest(new { message = "The username is used by another user" });
            }
            if (_usersService.UserEmailExists(Userid, userDto.Email))
            {
                return BadRequest(new { message = "The email is used by another user" });
            }

            userDto.Adapt(user);
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
                return NotFound(new { message = "User not found"});
            }
            await _usersService.DeleteUserAsync(user);
            return NoContent();
        }

        //-------------------------------- User Profile --------------------------------------
        //GET /api/users/profile
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> ViewUserProfile()
        {
            //extract user from token
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                return BadRequest(new { message = "Unauthorized user" });
            }
            var userId = Int32.Parse(userIdStr);
            var user = await _usersService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }
            var userDto = user.Adapt<UserDto>();
            return Ok(userDto);
        }

        // PUT /api/users/profile
        [Authorize]
        [HttpPut("profile")]
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
                return NotFound(new { message = "User not found"});
            }
            if (_usersService.UserNameExists(userId ,userDto.Username))
            {
                return BadRequest(new { message = "The username is used by another user" });
            }
            if (_usersService.UserEmailExists(userId, userDto.Email))
            {
                return BadRequest(new { message = "The email is used by another user" });
            }

            userDto.Adapt(user);
            int rowsAfected = await _usersService.PutUserAsync();
            if (rowsAfected > 0)
            {
                return Ok(new { message = "User updated successfully" });
            }
            else
            {
                return BadRequest(new { message = "Update failed" });
            }

                //return NoContent();
        }

        // DELETE /api/users/profile
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
                return NotFound(new { message = "User not found" });
            }
            await _usersService.DeleteUserAsync(user);
            Response.Cookies.Delete("jwtCookie");
            return NoContent();
        }

        // /api/users/questions
        [HttpPost("questions")]
        public async Task<IActionResult> ShowSecurityQuestions([FromBody] SecurityQuestionsAndAnswersDTO QuestionsDto)
        {
            string userName = QuestionsDto.Username;

            if (string.IsNullOrEmpty(userName))
            {
                return NotFound(new { message = "User not found" });
            }
            
            var user = await _usersService.GetUserByNameAsync(userName);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }
            if (!_usersService.SecurityAnswer(user, QuestionsDto))
            {
                return Unauthorized(new { message = "Unauthorized" });
            }
            
            return Ok(new { verified = true }); 
            
        }
    }
}
