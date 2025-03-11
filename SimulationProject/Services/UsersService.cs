using SimulationProject.Models;
using SimulationProject.Data;
using Microsoft.EntityFrameworkCore;
using SimulationProject.DTO;
using System.Text.RegularExpressions;

namespace SimulationProject.Services
{
   public interface IUsersService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(int Userid);
        Task<User> GetUserByNameAsync(string Username);
        Task CreateUserAsync(User user);
        abstract Task<int> PutUserAsync(User user, UserDto userDto);
        //Task UpDateUserAsync(User user);
        // Task DeleteUserAsync(User user);
        abstract Task DeleteUserAsync(User user, UserDto userDto);
        //Task<User?> RegisterUserAsync(RegisterForm registerForm);
        //Task<string?> LoginUserAsync(LoginForm loginform);
        bool UserExists(int Userid);
        bool UserNameExists(string Username);
        bool UserEmailExists(string Email);
        string GetUserNewPassword(PasswordUpdate PasswordUpdate, User user);
        Task UpdateUserPasswordAsync(string passwordHash, User user);
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
        public async Task<User> GetUserByIdAsync(int Userid)
        {
            return await _context.Users.FindAsync(Userid); 
        }

        // get by name
        public async Task<User> GetUserByNameAsync(string Username)
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
        public virtual async Task<int> PutUserAsync(User user)
        {
            int rowsAfected = 0;
            _context.Entry(user).State = EntityState.Modified;
            rowsAfected = await _context.SaveChangesAsync();
            return rowsAfected;
        }

        //update user profile
        public virtual async Task<int> PutUserAsync(User user, UserDto userDto)
        {
            return 0;
        }

        //delete
        public virtual async Task DeleteUserAsync(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteUserAsync(User user, UserDto userDto)
        {
            
        }

        //Get new password
        public string GetUserNewPassword(PasswordUpdate PasswordUpdate, User user)
        {

            string newpasswordHash = "";
            if (((!_passwordHashService.VerifyUserPassword(PasswordUpdate.OldPassword, user.Password)) || (user.Username != PasswordUpdate.UserName)))
            {
                newpasswordHash = "1";
            }
            if ((_passwordHashService.VerifyUserPassword(PasswordUpdate.NewPassword, user.Password)) || (!PasswordValid(PasswordUpdate.NewPassword)))
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
        public async Task UpdateUserPasswordAsync(string passwordHash, User user)
        {
            user.Password = passwordHash;
            _context.Entry(user).Property(u => u.Password).IsModified = true;
            await _context.SaveChangesAsync();
        }

        
    }
}
