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
using Azure;
using System.Data;
using Microsoft.EntityFrameworkCore.Metadata;

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
                expires: DateTime.UtcNow.AddMinutes(15),
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

        private Task<User?> ValidateRefreshTokenAsync(User user, string RefreshToken)
        {
            if ((user == null) || (user.Refreshtoken != RefreshToken) || user.Refreshtokenexpiry <= DateTime.UtcNow)
            {
                return Task.FromResult<User?>(null);
            }
            else
                return Task.FromResult<User?>(user);
        }

        //--------------- CSRF Token ----------------------
        public string GenerateCSRFToken()
        {
            var csrfToken = Guid.NewGuid().ToString();
            return csrfToken;
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

        public async Task<TokenDTo?>RefreshTokenAsync(User user,string request)
        {
            var validuser = await ValidateRefreshTokenAsync(user, request);
            if (validuser != null)
            {
                var response = new TokenDTo
                {
                    AccessToken = await GenerateAndSaveTokenAsync(validuser), //CreateJWToken(user),
                    RefreshToken = await GenerateAndSaveRefreshTokenAsync(validuser)
                };
                return response;
            }
            else
                return null;            
        }

        // Cookie with partitioned attribute for third party cookies
        public string SetPartitionedCookie(string name, string value, int maxAgeSeconds)
        {
            var cookieStr = $"{name}={value}; Max-Age={maxAgeSeconds}; Path=/; Secure; HttpOnly; SameSite=None; Partitioned";
            return cookieStr;
        }
        //----------------------------------------------------------

        //register user
        public async Task<User?> RegisterUserAsync(RegisterForm registerForm)
        {
            string passwordHash = _passwordHashService.HashUserPassword(registerForm.Password);
            string securityanswerHash = _passwordHashService.HashUserPassword(registerForm.Securityanswer);
            string securityanswerHash1 = _passwordHashService.HashUserPassword(registerForm.Securityanswer1);
            string securityanswerHash2 = _passwordHashService.HashUserPassword(registerForm.Securityanswer2);
           
            var user = registerForm.Adapt<User>();
            user.Role = FindUserRole(Convert.ToInt32(registerForm.Admin));
            user.Password = passwordHash;
            user.Securityanswer = securityanswerHash;
            user.Securityanswer1 = securityanswerHash1;
            user.Securityanswer2 = securityanswerHash2;

            try
            {
                await CreateUserAsync(user);
            }
            catch (DbUpdateException)
            {
                throw new DbUpdateException("Email or Username Exists.");
            }
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
                user.Loginattempt++;
                if (user.Loginattempt == 3)
                {
                    user.Active = false;
                }
                return null;
            }
            var response = new TokenDTo
            {
                AccessToken = await GenerateAndSaveTokenAsync(user), //CreateJWToken(user),
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)
            };
            user.Loginattempt = 0;
            return response;
        }
        // login user - check Active
        public async Task<bool> CheckUserActiveAsync(LoginForm loginform)
        {
            // Find user by username
            var user = await GetUserByNameAsync(loginform.UserName);
            return (user.Active || ((user.Emailtimestamp == null) || ((DateTime.UtcNow - user.Emailtimestamp) < TimeSpan.FromMinutes(10))));
        }
    }
}
