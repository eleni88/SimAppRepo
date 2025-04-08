namespace SimulationProject.DTO
{
    public class CreateUserDTO
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public int Age { get; set; }
        public string JobTitle { get; set; } = "";
        public bool Admin { get; set; }
    }
}
