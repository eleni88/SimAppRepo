namespace SimulationProject.DTO.UserDTOs
{
    public class PasswordReset
    {
        public string UserName { get; set; }
        public string TempPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
