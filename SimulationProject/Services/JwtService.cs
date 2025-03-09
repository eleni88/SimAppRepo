using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Azure;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using SimulationProject.Models;

namespace SimulationProject.Services
{
    public interface IJwtService
    {
        string CreateJWToken(User user);
    }
    public class JwtService(IConfiguration configuration) : IJwtService
    {
        public string CreateJWToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Userid.ToString())
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("Appsettings:Token")!));
            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("Appsettings:Issuer"),
                audience: configuration.GetValue<string>("Appsettings.Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1)
             );
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }            
    }
}
