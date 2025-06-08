using SimulationProject.DTO.RegionDTOs;
using SimulationProject.DTO.SimExecutionDTOs;

namespace SimulationProject.DTO.SimulationDTOs
{
    public class SimulationDTO
    {
        public int Simid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Codeurl { get; set; }
        public string Simparams { get; set; }
        public int Simuser { get; set; }
        public int Simcloud { get; set; }
        //public List<SimExecutionDTO> Simexecutions { get; set; }
        //public List<RegionDTO> Regions { get; set; }
    }
}
