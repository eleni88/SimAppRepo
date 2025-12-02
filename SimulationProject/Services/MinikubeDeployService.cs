using k8s;
using k8s.Models;
using SimulationProject.Helper.GitCloneHelper;
using SimulationProject.Helper.KubernetesHelper;
using SimulationProject.Models;
using YamlDotNet.RepresentationModel;
using static Azure.Core.HttpHeader;

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
            string kubeConfigContent = "";
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (userProfile != null)
            {
                var kubeConfigPath = Path.Combine(userProfile, ".kube", "config");
                if (File.Exists(kubeConfigPath))
                {
                    kubeConfigContent = await File.ReadAllTextAsync(kubeConfigPath);
                    KubeConfigValidator.ValidateKubeConfig(kubeConfigContent);
                }
            }           
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
                throw new Exception("No master Job found in repo YAMLs.");

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
                if (content.Contains("master") && content.Contains("Job"))
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
                        firstPort.Children["targetPort"] = new YamlScalarNode("8080");
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
                       (content.Contains("Job") || (content.Contains("Service") && (!content.Contains("ServiceAccount"))));
            }).ToList();

            // 7. Deploy configMap YAML
            await kubeClient.DeployYamlFilesAsync(new List<string> { configMapPath });
            // 8. RBAC 
            await RBACHelper.ApplyRbacAsync(kubeClient.GetClient());
            // 9. Deploy  master YAML
            await kubeClient.DeployYamlFilesAsync(masterYamlOnly);
            // 8. Poll master
            await _PollingService.WaitForSimulationToFinishAsync(newsimexec, kubeClient.GetClient(), "app=master", 0);

            try
            {
                // 9. Cleanup

                var deleteOptions = new V1DeleteOptions
                {
                    PropagationPolicy = "Foreground"
                };
                await kubeClient.GetClient().BatchV1.DeleteNamespacedJobAsync("master", "default", deleteOptions);
                await kubeClient.GetClient().CoreV1.DeleteNamespacedServiceAsync("master", "default");
                await kubeClient.GetClient().CoreV1.DeleteNamespacedConfigMapAsync("simulation-config", "default");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cleanup warning");
            }


            //}
            resultsJson = newsimexec.Execreport;
            return resultsJson;
        }
    }
}
