namespace SimulationProject.DTO
{
    public class UpdateUserDTO
    {
        public string Firstname { get; set; } = "";
        public string Lastname { get; set; } = "";
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public int Age { get; set; }
        public string Jobtitle { get; set; } = "";
        public bool Admin { get; set; }
    }
}
