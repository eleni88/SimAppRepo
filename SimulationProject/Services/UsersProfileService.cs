using SimulationProject.Data;
using SimulationProject.DTO;
using SimulationProject.Models;

namespace SimulationProject.Services
{
    public class UsersProfileService : UsersService
    {

        private readonly IPasswordHashService _passwordHashService;

        public UsersProfileService(SimSaasContext context,IPasswordHashService passwordHashService) : base(context,passwordHashService)
        {
            _passwordHashService = passwordHashService;
        }

        ////delete user profile
        //public override async Task DeleteUserAsync(User user, UserDto userDto)
        //{
        //    if (user.Securityanswer != null)
        //    {
        //        _passwordHashService.VerifyUserPassword(userDto.SecurityAnswer, user.Securityanswer);
        //        await base.DeleteUserAsync(user);
        //    }
        //}

        ////update user profile 
        //public override async Task<int> PutUserAsync(User user, UpdateUserProfileDTO userDto)
        //{
        //    int rowsAfected = 0;
        //    if (user.Securityanswer != null)
        //    {
        //        _passwordHashService.VerifyUserPassword(userDto.SecurityAnswer, user.Securityanswer);
        //        user.Firstname = userDto.FirstName;
        //        user.Lastname = userDto.LastName;
        //        user.Email = userDto.Email;
        //        user.Age = userDto.Age;
        //        user.Jobtitle = userDto.JobTitle;
        //        rowsAfected = await base.PutUserAsync();
        //    }
        //    return rowsAfected;
        //}
    }
}
