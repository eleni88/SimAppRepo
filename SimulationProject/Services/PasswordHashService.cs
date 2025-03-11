namespace SimulationProject.Services
{
    public interface IPasswordHashService
    {
        string HashUserPassword(string plainTextPassword);
        bool VerifyUserPassword(string plainTextPassword, string hashedPassword);
    }
    public class PasswordHashService: IPasswordHashService
    {
        public string HashUserPassword(string plainTextPassword)
        {
            return BCrypt.Net.BCrypt.HashPassword(plainTextPassword);
        }

        public bool VerifyUserPassword(string plainTextPassword, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(plainTextPassword, hashedPassword);
        }
    }
}
