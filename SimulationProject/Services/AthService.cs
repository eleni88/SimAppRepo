using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SimulationProject.Data;
using SimulationProject.Models;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Mapster;
using SimulationProject.DTO.UserDTOs;

namespace SimulationProject.Services
{
    public class AthService: UsersService
    {
        private readonly SimSaasContext _context;
        private readonly IConfiguration _configuration;
        private readonly IPasswordHashService _passwordHashService;

        public AthService(SimSaasContext context, IPasswordHashService passwordHashService, IConfiguration configuration) : base(context, passwordHashService)
        {
            _context = context;
            _configuration = configuration;
            _passwordHashService = passwordHashService;
        }
        //------------- JWT (Access Token) -------------------------
        private string CreateJWToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Userid.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("Appsettings:Token")!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration.GetValue<string>("Appsettings:Issuer"),
                audience: _configuration.GetValue<string>("Appsettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
             );
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        private async Task<string> GenerateAndSaveTokenAsync(User user)
        {
            var token = CreateJWToken(user);
            user.Jwtid = token;
            await _context.SaveChangesAsync();
            return token;
        }

        //--------------- Refresh Token ----------------------
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            var refreshToken = GenerateRefreshToken();
            user.Refreshtoken = refreshToken;
            user.Refreshtokenexpiry = DateTime.UtcNow.AddDays(1);
            await _context.SaveChangesAsync();
            return refreshToken;
        }

        private async Task<User?> ValidateRefreshTokenAsync(int UserId, string RefreshToken)
        {
            var user = await _context.Users.FindAsync(UserId);
            if ((user == null) || (user.Refreshtoken != RefreshToken) || user.Refreshtokenexpiry <= DateTime.UtcNow)
            {
                return null;
            }
            else
                return user;
        }

        //logout user
        public async Task<bool> RemoveRefreshTokenAsync(int UserId, string RefreshToken)
        {
            bool tokenrefreshed = false;
            var user = await _context.Users.FindAsync(UserId);
            if ((user != null) && (user.Refreshtoken == RefreshToken))
            {
                user.Refreshtoken = null;
                user.Refreshtokenexpiry = null;
                user.Jwtid = null;
                tokenrefreshed = await PutUserAsync() > 0;
            }
            return tokenrefreshed;
        }

        public async Task<TokenDTo?>RefreshTokenAsync(RefreshTokenDTo request)
        {
            var user = await ValidateRefreshTokenAsync(request.Userid, request.RefreshToken);
            if (user != null)
            {
                var response = new TokenDTo
                {
                    AccessToken = await GenerateAndSaveTokenAsync(user), //CreateJWToken(user),
                    RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)
                };
                return response;
            }
            else
                return null;            
        }

        //-------------- Create Cookie ---------------------------
        public CookieOptions GetCookieOptions()
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,                        // Make sure the cookie is not accessible via JavaScript (for security)
                Secure = true,                          // Ensures the cookie is only sent over HTTPS
                SameSite = SameSiteMode.None,           // Prevents cross-site request forgery (CSRF)
                Expires = DateTime.UtcNow.AddHours(2)   // Set the expiration of the cookie 
            };
            return cookieOptions;
        }
        //----------------------------------------------------------

        //register user
        public async Task<User?> RegisterUserAsync(RegisterForm registerForm)
        {
            string passwordHash = _passwordHashService.HashUserPassword(registerForm.Password);
            string securityanswerHash = _passwordHashService.HashUserPassword(registerForm.Securityanswer);

            var user = registerForm.Adapt<User>();
            user.Role = FindUserRole(Convert.ToInt32(registerForm.Admin));
            user.Password = passwordHash;
            user.Securityanswer = securityanswerHash;

            await CreateUserAsync(user);
            return user;
        }
        //login user
        public async Task<TokenDTo?> LoginUserAsync(LoginForm loginform)
        {
            // Find user by username
            var user = await GetUserByNameAsync(loginform.UserName);
            if (user == null)
            {
                return null;
            }
            // Verify password
            if (!_passwordHashService.VerifyUserPassword(loginform.Password, user.Password))
            {
                return null;
            }
            var response = new TokenDTo
            {
                AccessToken = await GenerateAndSaveTokenAsync(user), //CreateJWToken(user),
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)
            };
            return response;
        }
    }
}
