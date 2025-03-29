using SimulationProject.Data;
using SimulationProject.DTO;
using SimulationProject.Models;

namespace SimulationProject.Services
{
    public class AthService: UsersService
    {
        private readonly SimSaasContext _context;
        private IConfiguration _configuration;
        private readonly IPasswordHashService _passwordHashService;
        private readonly IJwtService _jwtService;

        public AthService(SimSaasContext context, IPasswordHashService passwordHashService, IConfiguration configuration, IJwtService jwtService) : base(context, passwordHashService)
        {
            _context = context;
            _configuration = configuration;
            _passwordHashService = passwordHashService;
            _jwtService = jwtService;
        }

        //find user role
        public string FindUserRole(int role)
        {
            string userRole;
            if (role == 1)
            {
                userRole = "Admin";
            }
            else
            {
                userRole = "User";
            }
            return userRole;
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
                Securityanswer = securityanswerHash,
                Role = FindUserRole(Convert.ToInt32(registerForm.Admin))
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
    }
}
