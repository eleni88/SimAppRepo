using SimulationProject.DTO.ProviderDTOs;
using SimulationProject.DTO.RegionDTOs;
using SimulationProject.DTO.ResourceDTOs;
using SimulationProject.DTO.SimExecutionDTOs;

namespace SimulationProject.DTO.SimulationDTOs
{
    public class SimulationDTO
    {
        public int Simid { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Createdate { get; set; }
        public DateTime Updatedate { get; set; }
        public string Codeurl { get; set; }
        public string Simparams { get; set; }
        public int Simuser { get; set; }
        public int Simcloud { get; set; }
        public ProviderDTO SimcloudNavigation { get; set; }
        public int Regionid { get; set; }
        public RegionDTO Region { get; set; }
        public List<SimExecutionDTO> Simexecutions { get; set; } = new();
        public ResourceDTO Resourcerequirement { get; set; } = new ResourceDTO();
    }
}
