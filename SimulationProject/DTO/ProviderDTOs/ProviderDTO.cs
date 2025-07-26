using SimulationProject.DTO.RegionDTOs;
using SimulationProject.DTO.SimExecutionDTOs;

namespace SimulationProject.DTO.ProviderDTOs
{
    public class ProviderDTO
    {
        public string Cloudid { get; set; }
        public string Name { get; set; } = "";
        public List<RegionDTO> Regions { get; set; } = new();

    }
}
