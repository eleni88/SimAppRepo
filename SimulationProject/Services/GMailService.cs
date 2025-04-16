using System.Text.Json;
using Microsoft.Extensions.Options;
using SimulationProject.DTO.UserDTOs;

namespace SimulationProject.Services
{
    public interface IGMailService
    {
        Task<string> GetAccessTokenAsync();
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
    }
}
