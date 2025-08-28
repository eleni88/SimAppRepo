using SimulationProject.DTO.UserDTOs;
using SimulationProject.Helper.GitCloneHelper;
using SimulationProject.Helper.KubernetesHelper;
using SimulationProject.Helper.TerraformHelper;
using SimulationProject.Models;
using YamlDotNet.RepresentationModel;

namespace SimulationProject.Services
{
    public class SimulationRunService
    {
        private readonly ILogger<ISimulationService> _logger;
        private readonly PollingService _PollingService;

        public SimulationRunService(ILogger<ISimulationService> logger, PollingService pollingService)
        {
            _logger = logger;
            _PollingService = pollingService;
        }

        public async Task<string> RunSimulationAsync(string repoUrl, string jsonParams, int Provider, string Region, string instanceType, int MinPods, int MaxPods, UserDto user, Simexecution newsimexec)
        {
            var OutputDirectory = Path.Combine("terraform_workdir", Guid.NewGuid().ToString("N"));
            var ClusterName = $"sim-cluster-{Guid.NewGuid().ToString("N").Substring(0, 6)}";
            int DesiredNodes = 1;

            string repoPath;
            try
            {
                repoPath = await GitHelper.CloneRepoAsync(repoUrl);
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
                string configMapPath = await ConfigMapGenerator.GenerateConfigMapFileAsync(jsonParams, tempDir);

                // Inject ConfigMap into master deployment
                foreach (var yaml in tmpYamlFiles)
                {
                    var content = File.ReadAllText(yaml);
                    if (content.Contains("master") && content.Contains("Deployment"))
                        YamlHelper.AddConfigMapToDeploymentYaml(yaml);
                    else
                    if (content.Contains("master") && content.Contains("Service"))
                    {
                        var stream = new YamlStream();
                        stream.Load(new StringReader(content));

                        if (stream.Documents[0].RootNode is YamlMappingNode root &&
                            root.Children.TryGetValue("spec", out var specNodeObj) &&
                            specNodeObj is YamlMappingNode specNode)
                        {
                            specNode.Children["type"] = new YamlScalarNode("NodePort");

                            if (!specNode.Children.TryGetValue("ports", out var portsNode) ||
                                portsNode is not YamlSequenceNode portsSeq)
                            {
                                portsSeq = new YamlSequenceNode();
                                specNode.Children["ports"] = portsSeq;
                            }

                            var firstPort = portsSeq.Children.OfType<YamlMappingNode>().FirstOrDefault();
                            if (firstPort == null)
                            {
                                firstPort = new YamlMappingNode();
                                portsSeq.Add(firstPort);
                            }

                            firstPort.Children["port"] = new YamlScalarNode("80");
                            firstPort.Children["targetPort"] = new YamlScalarNode("80");
                            firstPort.Children["nodePort"] = new YamlScalarNode("30080");
                            firstPort.Children["protocol"] = new YamlScalarNode("TCP");

                            using var writer = new StringWriter();
                            stream.Save(writer);
                            File.WriteAllText(yaml, writer.ToString());
                        }
                    }
                }

                // creation of terraform configuration
                if (Provider == 1)
                {
                    var builder = new TerraformBuilder()
                        .UseWorkingDirectory(OutputDirectory)
                        .AddRequiredProviders(Provider)
                        .AddAwsProvider(Region, user.Cloudcredentials.Find(prov => prov.Cloudid==Provider).Accesskeyid, 
                            user.Cloudcredentials.Find(prov => prov.Cloudid == Provider).Secretaccesskey)
                        .AddEksCluster(
                            ClusterName,
                            Region,
                            DesiredNodes,
                            MinPods,
                            MaxPods
                        );
                    await builder.CreateTerraformFile();
                }
                else
                if (Provider == 3)
                {
                    var builder = new TerraformBuilder()
                        .UseWorkingDirectory(OutputDirectory)
                        .AddRequiredProviders(Provider)
                        .AddAzureProvider(Region, user.Cloudcredentials.Find(prov => prov.Cloudid == Provider).Subscriptionid,
                            user.Cloudcredentials.Find(prov => prov.Cloudid == Provider).Clientid,
                            user.Cloudcredentials.Find(prov => prov.Cloudid == Provider).Clientsecret,
                            user.Cloudcredentials.Find(prov => prov.Cloudid == Provider).Tenantid)
                        .AddAzureCluster(
                            ClusterName,
                            Region,
                            DesiredNodes,
                            MinPods,
                            MaxPods
                        );
                    await builder.CreateTerraformFile();
                }
                

                var terraformRunner = new TerraformService(OutputDirectory);

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
                    // 2. Validate KubeConfig
                    KubeConfigValidator.ValidateKubeConfig(kubeConfig);
                    // 3. Deploy YAML files
                    var kubeClient = new KubernetesDeployerHelper(kubeConfig);
                    // RBAC 
                    await RBACHelper.ApplyRbacAsync(kubeClient.GetClient());
                    // 3.1. Deploy configMap YAML first
                    await kubeClient.DeployYamlFilesAsync(new List<string> { configMapPath });
                    // 3.2. Deploy the rest YAML files
                    await kubeClient.DeployYamlFilesAsync(tmpYamlFiles);
                    // 4. Wait until slave pods complete
                    await _PollingService.WaitForSimulationToFinishAsync(newsimexec, kubeClient.GetClient(), "app=slave", parsed.SlaveCount);

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
                        // 5. Destroy cluster
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
