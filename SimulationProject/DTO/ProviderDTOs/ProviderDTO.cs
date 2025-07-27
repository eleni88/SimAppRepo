using SimulationProject.DTO.RegionDTOs;

namespace SimulationProject.DTO.ProviderDTOs
{
    public class ProviderDTO
    {
        public string Cloudid { get; set; }
        public string Name { get; set; } = "";
        public List<RegionDTO> Regions { get; set; } = new();

    }
}
