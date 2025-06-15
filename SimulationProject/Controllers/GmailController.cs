using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimulationProject.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Net;
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

            if (string.IsNullOrEmpty(tempcodeRequest.username))
            {
                return NotFound(new { message = "User not found" });
            }
            string userName = tempcodeRequest.username;
            
            var user = await _usersService.GetUserByNameAsync(userName);    
            var userEmail = user.Email;

            var accessToken = await _tokenService.GetAccessTokenAsync();

            // Compose MIME email
            var mimeMessage = new StringBuilder();
            mimeMessage.AppendLine($"To: {userEmail}");
            mimeMessage.AppendLine("Your temporary code is: ");
            mimeMessage.AppendLine("Content-Type: text/plain; charset=utf-8");
            mimeMessage.AppendLine();
            mimeMessage.AppendLine(user.Tempcode);

            var raw = Convert.ToBase64String(Encoding.UTF8.GetBytes(mimeMessage.ToString()))
                        .Replace("+", "-").Replace("/", "_").Replace("=", "");

            var jsonPayload = JsonSerializer.Serialize(new { raw });
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.PostAsync("https://gmail.googleapis.com/gmail/v1/users/me/messages/send", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return Ok(new { message = "A temporary password has been sent to", to = userEmail });
            }

            return StatusCode((int)response.StatusCode, new { error = responseBody });
        }

    }
}
