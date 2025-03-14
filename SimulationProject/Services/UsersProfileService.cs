
using Microsoft.EntityFrameworkCore;
using SimulationProject.Data;
using SimulationProject.DTO;
using SimulationProject.Models;

namespace SimulationProject.Services
{
    public class UsersProfileService : UsersService
    {
        private readonly SimSaasContext _context;
        private IConfiguration _configuration;
        private readonly IPasswordHashService _passwordHashService;
        private readonly IJwtService _jwtService;
        public UsersProfileService(SimSaasContext context, IConfiguration configuration, IPasswordHashService passwordHashService, IJwtService jwtService) : base(context, configuration, passwordHashService, jwtService)
        {
            _context = context;
            _configuration = configuration;
            _passwordHashService = passwordHashService;
            _jwtService = jwtService;
        }

        //register user
        public async Task<User?> RegisterUserAsync(RegisterForm registerForm)
        {
            string passwordHash = _passwordHashService.HashUserPassword(registerForm.Password);
            string securityanswerHash = _passwordHashService.HashUserPassword(registerForm.SecurityAnswer);
            var user = new User
            {
                Username = registerForm.UserName,
                Password = passwordHash,
                Firstname = registerForm.FirstName,
                Lastname = registerForm.LastName,
                Email = registerForm.Email,
                Age = registerForm.Age,
                Jobtitle = registerForm.JobTitle,
                Admin = registerForm.Admin,
                Securityquestion = registerForm.SecurityQuestion,
                Securityanswer = securityanswerHash
            };
            await CreateUserAsync(user);
            return user;
        }
        //login user
        public async Task<string?> LoginUserAsync(LoginForm loginform)
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
            return _jwtService.CreateJWToken(user);
        }

        //delete user profile
        public override async Task DeleteUserAsync(User user, UserDto userDto)
        {
            if (user.Securityanswer != null)
            {
                _passwordHashService.VerifyUserPassword(userDto.SecurityAnswer, user.Securityanswer);
                await base.DeleteUserAsync(user);
            }
        }

        //update user profile 
        public override async Task<int> PutUserAsync(User user, UserDto userDto)
        {
            int rowsAfected = 0;
            if (user.Securityanswer != null)
            {
                _passwordHashService.VerifyUserPassword(userDto.SecurityAnswer, user.Securityanswer);
                user.Firstname = userDto.FirstName;
                user.Lastname = userDto.LastName;
                user.Email = userDto.Email;
                user.Age = userDto.Age;
                user.Jobtitle = userDto.JobTitle;
                rowsAfected = await base.PutUserAsync(user);
            }
            return rowsAfected;
        }
    }
}
