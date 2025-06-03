using SimulationProject.DTO.SimulationDTOs;

namespace SimulationProject.Helper.TerraformHelper
{
    public class TerraformInput
    {
        public string CloudProvider { get; set; }
        public string ClusterName { get; set; } = "simulation-cluster";
        public string AwsAccessKey { get; set; }
        public string AwsSecretKey { get; set; }
        public string Region { get; set; } = "us-west-1";
        public int MinNodes { get; set; } = 1;
        public int MaxNodes { get; set; } = 5;
        public string OutputDirectory { get; set; } = "./terraform-output";


        public static TerraformInput FromRequest(SimulationDTO request)
        {
            return new TerraformInput
            {
                //CloudProvider = request.Simcloud,
                //Region = request.Regions,
                //MinNodes = request.Regions,
                //MaxNodes = request.MaxInstances,
                //ClusterName = $"sim-cluster-{Guid.NewGuid().ToString("N").Substring(0, 6)}",
                //OutputDirectory = Path.Combine("terraform_workdir", Guid.NewGuid().ToString("N"))
            };
        }
    }
}
