using SimulationProject.DTO.InstanceTypeDTOs;
using SimulationProject.DTO.RegionDTOs;

namespace SimulationProject.DTO.ProviderDTOs
{
    public class ProviderDTO
    {
        public int Cloudid { get; set; }
        public string Name { get; set; } = "";
        public List<RegionDTO> Regions { get; set; } = new();
        public List<InstanceTypeDTO> Instancetypes { get; set; } = new();

    }
}
