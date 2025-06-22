using SimulationProject.Models;
using SimulationProject.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Mapster;
using SimulationProject.DTO.UserDTOs;
using SimulationProject.Helper;

namespace SimulationProject.Services
{
    public interface IUsersService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(int Userid);
        Task<User> GetUserByNameAsync(string Username);
        Task<User> GetUserByRefreshTokenAsync(string refreshtoken);
        Task CreateUserAsync(User user);
        Task<User?> CreateUserAsync(CreateUserDTO userdtto);
        Task<int> PutUserAsync();
        Task DeleteUserAsync(User user);
        bool SecurityAnswer(User user, SecurityQuestionsAndAnswersDTO QuestionsDto);
        bool UserExists(int Userid);
        bool UserNameExists(int Userid, string Username);
        bool UserEmailExists(int Userid, string Email);
        string GetUserNewPassword(string newPassword, string oldTempPassword, string userName, User user);
        Task UpdateUserPasswordAsync(string passwordHash, User user);
        Task<bool> UpdateSecurityQuestionAnsyc(string question, string question1, string question2, string answer, string answer1, string answer2, User user);
        Task<string> GenerateAndSaveTempCode(string username);
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

        public async Task<User?> CreateUserAsync(CreateUserDTO userdtto)
        {
            string passwordHash = _passwordHashService.HashUserPassword(userdtto.Password);
            var user = userdtto.Adapt<User>();
            user.Role = FindUserRole(Convert.ToInt32(userdtto.Admin));
            user.Password = passwordHash;
            await CreateUserAsync(user);
            return user;
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

        public async Task<User> GetUserByRefreshTokenAsync(string refreshtoken)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Refreshtoken == refreshtoken);
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
        public string GetUserNewPassword(string newPassword, string oldTempPassword, string userName, User user)
        {
            string newpasswordHash = string.Empty;

            // checks if the input oldPassword or the input username are same as the ones in the db 
            if ((_passwordHashService.VerifyUserPassword(oldTempPassword, user.Password)) && (user.Username == userName) &&
            // checks if the newPassword is different from the old one
                ((!_passwordHashService.VerifyUserPassword(newPassword, user.Password))))
            {
                newpasswordHash = _passwordHashService.HashUserPassword(newPassword);
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

        public async Task<string> GenerateAndSaveTempCode(string username)
        {
            var user = await GetUserByNameAsync(username);
            string tempcode = TempCodeGeneratorHelper.GenerateCode(10);
            user.Tempcode = tempcode;
            await _context.SaveChangesAsync();
            return tempcode;
        }

        //----------------------------------------------------------------------------------------------

        //----------------------- security questions -----------------------------------

        public bool SecurityAnswer(User user, SecurityQuestionsAndAnswersDTO QuestionsDto)
        {
            return((user.Securityanswer != null) && (QuestionsDto.Securityquestion != null) && (QuestionsDto.Securityanswer != null) &&
                    _passwordHashService.VerifyUserPassword(QuestionsDto.Securityanswer, user.Securityanswer));
        }

        // update Sequrity Questions
        public async Task<bool> UpdateSecurityQuestionAnsyc(string question, string question1, string question2, string answer, string answer1, string answer2, User user)
        {
            user.Securityquestion = question;
            user.Securityanswer = _passwordHashService.HashUserPassword(answer);
            user.Securityquestion1 = question1;
            user.Securityanswer1 = _passwordHashService.HashUserPassword(answer1);
            user.Securityquestion2 = question2;
            user.Securityanswer2 = _passwordHashService.HashUserPassword(answer2);
            return await PutUserAsync()>0;
        }
    }
}
