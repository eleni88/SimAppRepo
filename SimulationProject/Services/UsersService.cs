using SimulationProject.Models;
using SimulationProject.Data;
using Microsoft.EntityFrameworkCore;
using SimulationProject.DTO;
using Microsoft.AspNetCore.Http.HttpResults;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;
using System.Text.RegularExpressions;

namespace SimulationProject.Services
{
    public interface IUsersService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int Userid);
        Task<User?> GetUserByNameAsync(string Username);
        Task CreateUserAsync(User user);
        Task PutUserAsync(int Userid, User user);
        Task DeleteUserAsync(User user);
        Task<User?> RegisterUserAsync(RegisterForm registerForm);
        Task<string?> LoginUserAsync(LoginForm loginform);
        bool UserExists(int Userid);
        bool UserNameExists(string Username);
        bool UserEmailExists(string Email);
        string GetUserNewPassword(PasswordUpdate PasswordUpdate, string upassword, string uname);
        Task UpdateUserPasswordAsync(User user);
        bool PasswordValid(string password);
    }
    public class UsersService: IUsersService
    {
        private readonly SimSaasContext _context;
        private IConfiguration _configuration;
        private readonly IPasswordHashService _passwordHashService;
        private readonly IJwtService _jwtService;
        public UsersService(SimSaasContext context, IConfiguration configuration, IPasswordHashService passwordHashService, IJwtService jwtService)
        {
            _context = context;
            _configuration = configuration;
            _passwordHashService = passwordHashService;
            _jwtService = jwtService;
        }

        public bool UserEmailExists(string Email)
        {
            return _context.Users.Any(e => e.Email == Email);
        }

        public bool PasswordValid(string password)
        {
            // Regular expression for validating the password strength
            const string PasswordRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#^])[A-Za-z\d@$!%*?&#^]{10,}$";
            return (!Regex.IsMatch(password, PasswordRegex));
        }

        public bool UserExists(int Userid)
        {
            return _context.Users.Any(e => e.Userid == Userid);
        }

        public bool UserNameExists(string Username)
        {
            return _context.Users.Any(e => e.Username == Username);
        }
        // get
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        //get by id
        public async Task<User?> GetUserByIdAsync(int Userid)
        {
            return await _context.Users.FindAsync(Userid); 
        }

        // get by name
        public async Task<User?> GetUserByNameAsync(string Username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == Username);
        }

        //post
        public async Task CreateUserAsync(User user)
        {   
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        //put
        public async Task PutUserAsync(int Userid, User user)
        {
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        //delete
        public async Task DeleteUserAsync(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        //Get new password
        public string GetUserNewPassword(PasswordUpdate PasswordUpdate, string upassword, string uname)
        {
            string newpasswordHash = "";
            if (((!_passwordHashService.VerifyUserPassword(upassword, PasswordUpdate.OldPassword)) || (uname != PasswordUpdate.UserName)))
            {
                newpasswordHash = "1";
            }
            if ((_passwordHashService.VerifyUserPassword(upassword, PasswordUpdate.NewPassword)) || (!PasswordValid(PasswordUpdate.NewPassword)))
            {
                newpasswordHash = "2";
            }
            if (PasswordUpdate.NewPassword == PasswordUpdate.ConfirmPassword)
            {
                newpasswordHash = "3";
            }
            if (newpasswordHash == "")
            {
                newpasswordHash = _passwordHashService.HashUserPassword(PasswordUpdate.NewPassword);                
            }
            return newpasswordHash;
        }

        //update password
        public async Task UpdateUserPasswordAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        //register user
        public async Task<User?> RegisterUserAsync(RegisterForm registerForm)
        {
            string passwordHash = _passwordHashService.HashUserPassword(registerForm.Password);
            var user = new User
            {
                Username = registerForm.UserName,
                Password = passwordHash,
                Firstname = registerForm.FirstName,
                Lastname = registerForm.LastName,
                Email = registerForm.Email,
                Age = registerForm.Age,
                Jobtitle = registerForm.JobTitle,
                Admin = registerForm.Admin
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
            if (!_passwordHashService.VerifyUserPassword(user.Password, loginform.Password))
            {
                return null;
            }
            return _jwtService.CreateJWToken(user);
        }
    }
}
