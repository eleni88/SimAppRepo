using Microsoft.AspNetCore.Mvc;
using SimulationProject.Services;
using Microsoft.AspNetCore.Authorization;
using SimulationProject.DTO.UserDTOs;

namespace SimulationProject.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class GmailController : ControllerBase
    {
        private readonly IGMailService _tokenService;
        private readonly IUsersService _usersService;

        public GmailController(IGMailService tokenService, IUsersService usersService)
        {
            _tokenService = tokenService;
            _usersService = usersService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendEmailToUser([FromBody] TempcodeRequestDTO tempcodeRequest)
        {
            string userName = tempcodeRequest.username;
            
            var user = await _usersService.GetUserByNameAsync(userName);
            if (user == null)
            {
                return Ok(new { message = "A temporary password has been sent" });
            }

            var userEmail = user.Email;

            var TmpCode = user.Tempcode;
            if (string.IsNullOrEmpty(TmpCode))
            {
                return BadRequest("No temporary code is set for this user.");
            }
            try
            {
                await _tokenService.SendEmailAsync(userEmail, TmpCode);
                return Ok(new { message = $"A temporary password has been sent to {userEmail}", to = userEmail });
            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Failed to send email: {ex.Message}");
            }
        }

    }
}
