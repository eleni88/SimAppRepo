using SimulationProject.Data;
using SimulationProject.DTO.RegionDTOs;
using SimulationProject.DTO.ResourceDTOs;
using SimulationProject.DTO.SimulationDTOs;
using SimulationProject.Services;

namespace SimulationProject.Helper.TerraformHelper
{
    public class TerraformInput
    {
        public int CloudProvider { get; set; }
        public int Providerid { get; set; }
        public string ClusterName { get; set; } = "simulation-cluster";
        public string AwsAccessKey { get; set; }
        public string AwsSecretKey { get; set; }
        public string Region { get; set; } = "us-west-1";
        public int DesiredNodes { get; set; } = 1;
        public int MinNodes { get; set; } = 1;
        public int MaxNodes { get; set; } = 5;
        public string OutputDirectory { get; set; } = "./terraform-output";


        public static TerraformInput FromRequest(SimulationDTO request, RegionDTO region, ResourceDTO resource )
        {

            return new TerraformInput
            {
                CloudProvider = request.Simcloud,
                Region = region.Regioncode,
                MinNodes = resource.Mininstances,
                MaxNodes = resource.Maxinstances,
                DesiredNodes = resource.Mininstances,
                ClusterName = $"sim-cluster-{Guid.NewGuid().ToString("N").Substring(0, 6)}",
                OutputDirectory = Path.Combine("terraform_workdir", Guid.NewGuid().ToString("N"))
            };
        }
    }
}
