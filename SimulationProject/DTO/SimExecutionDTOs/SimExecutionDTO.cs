namespace SimulationProject.DTO.SimExecutionDTOs
{
    public class SimExecutionDTO
    {
        public string State { get; }
        public float Cost { get; } 
        public DateTime Startdate { get; } 
        public DateTime Enddate { get; }
        public float Duration { get; }
        public BinaryData Execreport { get; }
    }
}
