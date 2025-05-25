namespace SimulationProject.DTO.SimulationDTOs
{
    public class CreateSimulationDTO
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Codeurl { get; set; } = "";
        public string Simparams { get; set; } = "";
        public int Simcloud { get; set; }
        public DateTime Createdate { get; set; }
        public DateTime Uodatedate {get; set; }
    }
}
