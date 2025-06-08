using SimulationProject.Helper.GitCloneHelper;
using SimulationProject.Helper.KubernetesHelper;
using SimulationProject.Helper.TerraformHelper;

namespace SimulationProject.Services
{
    public class SimulationRunService
    {
        //private readonly ILogger<SimulationService> _logger;

        //public SimulationRunService(ILogger<SimulationService> logger)
        //{
        //    _logger = logger;
        //}

        //public async Task<string> RunSimulationAsync(SimulationRequest request)
        //{
        //    string repoPath;
        //    try
        //    {
        //        repoPath = await GitHelper.CloneRepoAsync(request.GitUrl);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Git clone failed: {Message}", ex.Message);
        //        throw new Exception("Repository cloning failed.");
        //    }

        //    var yamlFiles = YamlHelper.FindYamlFiles(repoPath);
        //    if (!yamlFiles.Any())
        //        throw new Exception("No YAML files found in repository.");

        //    var parsed = YamlHelper.ParseYamlFiles(yamlFiles);
        //    if (!parsed.HasMaster || parsed.SlaveCount == 0)
        //        throw new Exception("Invalid or missing master/slave definitions in YAML.");

        //    var terraformInput = TerraformInput.FromRequest(request);
        //    TerraformBuilder.GenerateAwsEksTemplate(terraformInput);

        //    var terraformRunner = new TerraformService(terraformInput.OutputDirectory);
        //    var initResult = await terraformRunner.InitAsync();
        //    if (!initResult.Success)
        //        throw new Exception("Terraform init failed: " + initResult.Error);

        //    var applyResult = await terraformRunner.ApplyAsync();
        //    if (!applyResult.Success)
        //        throw new Exception("Terraform apply failed: " + applyResult.Error);

        //    // Deploy YAMLs to Kubernetes
        //    try
        //    {
        //        var kubeConfigPath = await TerraformOutputParser.ExtractKubeconfig(terraformInput.OutputDirectory);
        //        var kubeClient = new KubernetesDeployerHelper(kubeConfigPath);
        //        await kubeClient.DeployYamlFilesAsync(yamlFiles);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("YAML deployment failed: {Message}", ex.Message);
        //        await terraformRunner.DestroyAsync(); // Cleanup
        //        throw new Exception("Deployment to Kubernetes cluster failed.");
        //    }

        //    return "Simulation started successfully.";
        //}
    }
}
