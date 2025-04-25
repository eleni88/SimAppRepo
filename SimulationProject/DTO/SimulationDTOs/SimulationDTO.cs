using SimulationProject.DTO.SimExecutionDTOs;

namespace SimulationProject.DTO.SimulationDTOs
{
    public class SimulationDTO
    {
        public string Name { get; }
        public string Description { get; }
        public string Codeurl { get; }
        public string Simparams { get; }
        public int Simuser { get; }
        public int Simcloud { get; }
        public List<SimExecutionDTO> simExecutions { get; }
    }
}
