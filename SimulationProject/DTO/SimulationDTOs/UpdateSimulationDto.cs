using SimulationProject.DTO.RegionDTOs;

namespace SimulationProject.DTO.SimulationDTOs
{
    public class UpdateSimulationDTO
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Codeurl { get; set; } = "";
        public string Simparams { get; set; } = "";
        public int Simcloud { get; set; }
        public int Regionid { get; set; }
    }

}
