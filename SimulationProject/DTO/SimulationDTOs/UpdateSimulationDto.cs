namespace SimulationProject.DTO.SimulationDTOs
{
    public class UpdateSimulationDto
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Codeurl { get; set; } = "";
        public string Simparams { get; set; } = "";
        public int Simcloud { get; set; }
    }
}
