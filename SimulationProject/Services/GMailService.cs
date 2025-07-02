using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SimulationProject.DTO.UserDTOs;

namespace SimulationProject.Services
{
    public interface IGMailService
    {
        Task<string> GetAccessTokenAsync();
        Task SendEmailAsync(string emailTo, string tmpcode);
    }
    public class GMailService: IGMailService
    {
        private readonly GMailSettings _settings;

        public GMailService(IOptions<GMailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var body = new Dictionary<string, string>
        {
            { "client_id", _settings.ClientId },
            { "client_secret", _settings.ClientSecret },
            { "refresh_token", _settings.RefreshToken },
            { "grant_type", "refresh_token" }
        };

            using var client = new HttpClient();
            var tokenResponse = await client.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(body));
            var json = await tokenResponse.Content.ReadAsStringAsync();

            if (!tokenResponse.IsSuccessStatusCode)
                throw new Exception($"Failed to refresh Gmail access token: {json}");

            var parsed = JsonDocument.Parse(json);
            return parsed.RootElement.GetProperty("access_token").GetString();
        }

        private async Task<HttpResponseMessage> SendGmailApiCall(string accessToken, string emailTo, string tmpcode)
        {
            // Compose MIME email
            var mimeMessage = new StringBuilder();
            mimeMessage.AppendLine($"To: {emailTo}");
            mimeMessage.AppendLine("Subject: Temporary code");
            mimeMessage.AppendLine("Content-Type: text/plain; charset=utf-8");
            mimeMessage.AppendLine();
            mimeMessage.AppendLine($"Your temporary code is: {tmpcode}");

            var raw = Convert.ToBase64String(Encoding.UTF8.GetBytes(mimeMessage.ToString()))
                        .Replace("+", "-").Replace("/", "_").Replace("=", "");

            var jsonPayload = JsonSerializer.Serialize(new { raw });
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            return await client.PostAsync("https://gmail.googleapis.com/gmail/v1/users/me/messages/send", content);
        }

        public async Task SendEmailAsync(string emailTo, string tmpcode)
        {
            var accessToken = await GetAccessTokenAsync();
            var response = await SendGmailApiCall(accessToken, emailTo, tmpcode);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                accessToken = await GetAccessTokenAsync();
                response = await SendGmailApiCall(accessToken, emailTo, tmpcode);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new Exception("Gmail API returned 401 even after refreshing access token.");
                }
            }
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Gmail API failed: {error}");
            }
        }
    }
}
