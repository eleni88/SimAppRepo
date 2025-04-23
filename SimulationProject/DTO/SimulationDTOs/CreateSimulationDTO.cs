namespace SimulationProject.DTO.SimulationDTOs
{
    public class CreateSimulationDTO
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime Updatedate { get; set; }
        public string Codeurl { get; set; } = "";
        public string Simparams { get; set; } = "";
        public int Simuser { get; set; }
        public int Simcloud { get; set; } 
    }
}
