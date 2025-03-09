namespace SimulationProject.Services
{
    public interface IPasswordHashService
    {
        string HashUserPassword(string password);
        bool VerifyUserPassword(string hashedPassword, string password);
    }
    public class PasswordHashService: IPasswordHashService
    {
        public string HashUserPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyUserPassword(string hashedPassword, string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
