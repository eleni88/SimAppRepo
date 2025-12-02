using SimulationProject.DTO.ResourceDTOs;

namespace SimulationProject.DTO.SimulationDTOs
{
    public class CreateSimulationDTO
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Codeurl { get; set; } = "";
        public string Simparams { get; set; } = "";
        public int Simcloud { get; set; }
        public int Regionid { get; set; }
        public int Resourceid { get; set; }
        public UpdateResourceDTO Resourcerequirement { get; set; } = new UpdateResourceDTO();
    }
}
