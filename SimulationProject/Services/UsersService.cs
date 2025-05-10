using SimulationProject.Models;
using SimulationProject.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Mapster;
using SimulationProject.DTO.UserDTOs;

namespace SimulationProject.Services
{
    public interface IUsersService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(int Userid);
        Task<User> GetUserByNameAsync(string Username);
        Task CreateUserAsync(User user);
        Task<int> PutUserAsync();
        Task DeleteUserAsync(User user);
        bool SecurityAnswer(User user, SecurityQuestionsAndAnswersDTO QuestionsDto);
        bool UserExists(int Userid);
        bool UserNameExists(int Userid, string Username);
        bool UserEmailExists(int Userid, string Email);
        string GetUserNewPassword(PasswordUpdate PasswordUpdate, User user);
        Task UpdateUserPasswordAsync(string passwordHash, User user);
        bool PasswordValid(string password);
        string FindUserRole(int role);
    }
    public class UsersService: IUsersService
    {
        private readonly SimSaasContext _context;
        private readonly IPasswordHashService _passwordHashService;

        public UsersService(SimSaasContext context, IPasswordHashService passwordHashService)
        {
            _context = context;
            _passwordHashService = passwordHashService;
        }

        public bool UserEmailExists(int Userid, string Email)
        {
            return _context.Users.Any(e => ((e.Userid != Userid) && (e.Email == Email)));
        }

        public bool UserExists(int Userid)
        {
            return _context.Users.Any(e => e.Userid == Userid);
        }

        public bool UserNameExists(int Userid,string Username)
        {
            return _context.Users.Any(e => ((e.Userid != Userid) && (e.Username == Username)));
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
        public async Task<int> PutUserAsync()
        {
            int rowsAfected = 0;
            rowsAfected = await _context.SaveChangesAsync();
            return rowsAfected;
        }

        //delete
        public async Task DeleteUserAsync(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        //---------------------- Password ------------------------------------
        //Check Password validity
        public bool PasswordValid(string password)
        {
            // Regular expression for validating the password strength
            const string PasswordRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#^])[A-Za-z\d@$!%*?&#^]{10,}$";
            return Regex.IsMatch(password, PasswordRegex);
        }

        //Get new password
        public string GetUserNewPassword(PasswordUpdate PasswordUpdate, User user)
        {
            string newpasswordHash = "";
            if (((!_passwordHashService.VerifyUserPassword(PasswordUpdate.OldPassword, user.Password)) || (user.Username != PasswordUpdate.UserName)))
            {
                newpasswordHash = "1";
            }
            else
            if ((_passwordHashService.VerifyUserPassword(PasswordUpdate.NewPassword, user.Password)) || (!PasswordValid(PasswordUpdate.NewPassword)))
            {
                newpasswordHash = "2";
            }
            else
            if (PasswordUpdate.NewPassword != PasswordUpdate.ConfirmPassword)
            {
                newpasswordHash = "3";
            }
            else
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

        //----------------------------------------------------------------------------------------------

        //----------------------- security questions -----------------------------------

        public bool SecurityAnswer(User user, SecurityQuestionsAndAnswersDTO QuestionsDto)
        {
            return((user.Securityanswer != null) && (QuestionsDto.Securityquestion != null) && (QuestionsDto.Securityanswer != null) &&
                    _passwordHashService.VerifyUserPassword(QuestionsDto.Securityanswer, user.Securityanswer));
        }
    }
}
