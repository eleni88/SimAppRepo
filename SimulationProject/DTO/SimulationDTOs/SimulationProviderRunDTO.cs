using SimulationProject.DTO.ProviderDTOs;
using SimulationProject.DTO.RegionDTOs;
using SimulationProject.DTO.ResourceDTOs;
using SimulationProject.DTO.SimExecutionDTOs;

namespace SimulationProject.DTO.SimulationDTOs
{
    public class SimulationProviderRunDTO
    {
        public int Simid { get; set; }
        public string Codeurl { get; set; }
        public string Simparams { get; set; }
        public int Simcloud { get; set; }
        public ProviderDTO SimcloudNavigation { get; set; }
        public int Regionid { get; set; }
        public RegionDTO Region { get; set; }
        public ResourceDTO Resourcerequirement { get; set; } = new ResourceDTO();
    }
}
