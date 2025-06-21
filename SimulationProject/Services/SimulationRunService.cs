using System.Diagnostics;
using System.Text.Json;
using SimulationProject.Data;
using SimulationProject.DTO.RegionDTOs;
using SimulationProject.DTO.ResourceDTOs;
using SimulationProject.DTO.SimulationDTOs;
using SimulationProject.Helper.GitCloneHelper;
using SimulationProject.Helper.KubernetesHelper;
using SimulationProject.Helper.TerraformHelper;

namespace SimulationProject.Services
{
    public class SimulationRunService
    {
        private readonly ILogger<SimulationService> _logger;
        private readonly PollingService _PollingService;

        public SimulationRunService(ILogger<SimulationService> logger, PollingService pollingService)
        {
            _logger = logger;
            _PollingService = pollingService;
        }

        public async Task<string> RunSimulationAsync(SimulationDTO request, RegionDTO region, ResourceDTO resource)
        {
            string repoPath;
            try
            {
                repoPath = await GitHelper.CloneRepoAsync(request.Codeurl);
            }
            catch (Exception ex)
            {
                _logger.LogError("Git clone failed: {Message}", ex.Message);
                throw new Exception("Repository cloning failed.");
            }
            // create a temporary folder to copy all yamls needed from repo
            var tempDir = Path.Combine("/tmp/deployment", Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                var yamlFiles = YamlHelper.FindYamlFiles(repoPath);
                if (!yamlFiles.Any())
                    throw new Exception("No YAML files found in repository.");

                var parsed = YamlHelper.ParseYamlFiles(yamlFiles);
                if (!parsed.HasMaster || parsed.SlaveCount == 0)
                    throw new Exception("Invalid or missing master/slave definitions in YAML.");

                // Copy only needed YAMLs
                var tmpYamlFiles = YamlHelper.CopyYamlFilesToTmp(yamlFiles, tempDir);
                // create configMap yaml
                string configMapPath = await ConfigMapGenerator.GenerateConfigMapFileAsync(request.Simparams, tempDir);

                // creation of terraform configuration
                var terraformInput = TerraformInput.FromRequest(request, region, resource);

                var builder = new TerraformBuilder()
                    .UseWorkingDirectory(terraformInput.OutputDirectory)
                    .AddRequiredProviders()
                    .AddAwsProvider(terraformInput.Region, terraformInput.AwsAccessKey, terraformInput.AwsSecretKey)
                    .AddEksCluster(
                        terraformInput.ClusterName,
                        terraformInput.Region,
                        terraformInput.DesiredNodes,
                        terraformInput.MinNodes,
                        terraformInput.MaxNodes
                    );
                await builder.CreateTerraformFile();

                var terraformRunner = new TerraformService(terraformInput.OutputDirectory);

                // Terraform init
                var initResult = await terraformRunner.InitAsync();
                if (!initResult.Success)
                {
                    _logger.LogError("Terraform init failed: {Error}", initResult.Error);
                    throw new Exception("Terraform init failed: " + initResult.Error);
                }

                // Terraform apply
                var applyResult = await terraformRunner.ApplyAsync();
                if (!applyResult.Success)
                {
                    _logger.LogError("Terraform apply failed: {Error}", applyResult.Error);
                    throw new Exception("Terraform apply failed: " + applyResult.Error);
                }

                // Deploy YAMLs to Kubernetes
                try
                {
                    // 1. Get kubeconfig
                    var kubeConfig = await terraformRunner.GetKubeConfigAsync();
                    // 2. Deploy YAML files
                    var kubeClient = new KubernetesDeployerHelper(kubeConfig);
                    // Deploy configMap YAML first
                    await kubeClient.DeployYamlFilesAsync(new List<string> { configMapPath });
                    // Deploy the rest YAML files
                    await kubeClient.DeployYamlFilesAsync(tmpYamlFiles);
                    // 3. Wait until slave pods complete
                    await _PollingService.WaitForSimulationToFinishAsync(kubeClient.GetClient(), "app=slave", parsed.SlaveCount);

                    return "Simulation completed and cluster destroyed.";
                }
                catch (Exception ex)
                {
                    _logger.LogError("YAML deployment failed: {Message}", ex.Message);
                    throw new Exception("Deployment to Kubernetes cluster failed.");
                }
                finally
                {
                    try
                    {
                        // 4. Destroy cluster
                        await terraformRunner.DestroyAsync();
                    }
                    catch (Exception destroyEx)
                    {
                        _logger.LogWarning("Failed to destroy cluster: {Message}", destroyEx.Message);
                    }
                }
            }
            finally
            {
                try
                {
                    if (Directory.Exists(tempDir))
                        Directory.Delete(tempDir, recursive: true);
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogWarning("Could not delete temp folder: {Path}, error: {Message}", tempDir, cleanupEx.Message);
                }
            }
        }
    }
}
