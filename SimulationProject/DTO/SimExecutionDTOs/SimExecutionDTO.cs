namespace SimulationProject.DTO.SimExecutionDTOs
{
    public class SimExecutionDTO
    {
        public int Execid { get; set; }
        public string State { get; set; }
        public float Cost { get; set; } 
        public DateTime Startdate { get; set; } 
        public DateTime Enddate { get; set; }
        public string Duration { get; set; }
        public string Execreport { get; set; }
    }
}
