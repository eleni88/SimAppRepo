namespace SimulationProject.DTO.UserDTOs
{
    public class RegisterForm
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public int Age { get; set; }
        public string Organization { get; set; }
        public string Jobtitle { get; set; }
        public bool Admin { get; set; }
        public string Securityquestion { get; set; }
        public string Securityanswer { get; set; }
    }
}
