namespace SimulationProject.DTO.SimExecutionDTOs
{
    public class SimExecutionDTO
    {
        public int Execid { get; set; }
        public string State { get; set; }
        public float Cost { get; set; } 
        public DateTime Startdate { get; set; } 
        public DateTime Enddate { get; set; }
        public float Duration { get; set; }
        //public BinaryData Execreport { get; set; }
    }
}
