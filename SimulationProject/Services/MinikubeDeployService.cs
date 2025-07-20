using k8s;
using SimulationProject.Helper.GitCloneHelper;
using SimulationProject.Helper.KubernetesHelper;
using SimulationProject.Models;
using YamlDotNet.RepresentationModel;

namespace SimulationProject.Services
{
    public class MinikubeDeployService
    {
        private readonly ILogger<ISimulationService> _logger;
        private readonly PollingService _PollingService;
        public MinikubeDeployService(ILogger<ISimulationService> logger, PollingService pollingService)
        {
            _logger = logger;
            _PollingService = pollingService;
        }
        public async Task<string> RunSimulationToMinikubeAsync(string repoUrl, string jsonParams, Simexecution newsimexec)
        {
            var logger = new LoggerFactory().CreateLogger("MinikubeDeploy");
            var kubeConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".kube", "config");
            var kubeConfigContent = await File.ReadAllTextAsync(kubeConfigPath);
            var kubeClient = new KubernetesDeployerHelper(kubeConfigContent);

            string resultsJson = "";

            // 1. Clone repo
            var repoPath = await GitHelper.CloneRepoAsync(repoUrl);

            // 2. Find YAML files
            var allYamlFiles = YamlHelper.FindYamlFiles(repoPath);
            if (!allYamlFiles.Any())
                throw new Exception("No YAML files found in repository.");
            var parsed = YamlHelper.ParseYamlFiles(allYamlFiles);
            if (!parsed.HasMaster)
                throw new Exception("No master deployment found in repo YAMLs.");

            // 3. Copy YAMLs to temp folder
            var tempDir = Path.Combine("/tmp/sim-mk", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            var yamlFilesToDeploy = YamlHelper.CopyYamlFilesToTmp(allYamlFiles, tempDir);

            // 4. Generate configMap
            var configMapPath = await ConfigMapGenerator.GenerateConfigMapFileAsync(jsonParams, tempDir);

            // 5. Inject ConfigMap into master deployment
            foreach (var yaml in yamlFilesToDeploy)
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

            // 6. Filter only master deployment and service YAMLs
            var masterYamlOnly = yamlFilesToDeploy.Where(yaml =>
            {
                var content = File.ReadAllText(yaml);
                return content.Contains("master") &&
                       (content.Contains("Deployment") || content.Contains("Service"));
            }).ToList();

            // 7. Deploy configMap and master-only YAMLs
            await kubeClient.DeployYamlFilesAsync(new List<string> { configMapPath });
            await kubeClient.DeployYamlFilesAsync(masterYamlOnly);

            // 8. Poll master
            await _PollingService.WaitForSimulationToFinishAsync(newsimexec, kubeClient.GetClient(), "app=master", 0);

            // 9.Fetch results from master service
        try
            {
                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync("http://localhost:30080/results"); //  NodePort 
                if (response.IsSuccessStatusCode)
                {
                    resultsJson = await response.Content.ReadAsStringAsync();
                    logger.LogInformation("Simulation Results (JSON):\n{Results}", resultsJson);
                }
                else
                {
                    logger.LogWarning("Failed to fetch results. Status: {Status}", response.StatusCode);
                    resultsJson = $"Error: Failed to fetch results. Status {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving results from master service");
                resultsJson = $"Exception: {ex.Message}";
            }

            // 10. Cleanup
            try
            {
                await kubeClient.GetClient().AppsV1.DeleteNamespacedDeploymentAsync("master", "default");
                await kubeClient.GetClient().CoreV1.DeleteNamespacedServiceAsync("master", "default");
                await kubeClient.GetClient().CoreV1.DeleteNamespacedConfigMapAsync("simulation-config", "default");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Cleanup warning");
            }

            return resultsJson;
        }
    }
}
