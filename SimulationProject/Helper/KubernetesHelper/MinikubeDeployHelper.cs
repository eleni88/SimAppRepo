using k8s;
using SimulationProject.Helper.GitCloneHelper;
using SimulationProject.Services;

namespace SimulationProject.Helper.KubernetesHelper
{
    public static class MinikubeDeployHelper
    {
        public static async Task<string> RunSimulationToMinikubeAsync(string repoUrl, string jsonParams)
        {
            var kubeConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".kube", "config");
            var kubeConfigContent = await File.ReadAllTextAsync(kubeConfigPath);
            var kubeClient = new KubernetesDeployerHelper(kubeConfigContent);

            string resultsJson = "";

            // 1. Clone repo
            var repoPath = await GitHelper.CloneRepoAsync(repoUrl);

            // 2. Find YAML files
            var allYamlFiles = YamlHelper.FindYamlFiles(repoPath);
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
            }

            // 6. Deploy
            await kubeClient.DeployYamlFilesAsync(new List<string> { configMapPath });
            await kubeClient.DeployYamlFilesAsync(yamlFilesToDeploy);

            // 7. Poll master
            var polling = new PollingService(new LoggerFactory().CreateLogger<SimulationService>());
            await polling.WaitForSimulationToFinishAsync(kubeClient.GetClient(), "app=master", 0);

            // 8. Fetch results from master service
            try
            {
                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync("http://localhost:30080/results"); // Assuming port-forward
                if (response.IsSuccessStatusCode)
                {
                    resultsJson = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("\nSimulation Results (JSON):\n" + resultsJson);
                }
                else
                {
                    Console.WriteLine($"Failed to fetch results. Status: {response.StatusCode}");
                    resultsJson = $"Error: Failed to fetch results. Status {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving results from master service: {ex.Message}");
                resultsJson = $"Exception: {ex.Message}";
            }

            // 9. Cleanup
            try
            {
                await kubeClient.GetClient().AppsV1.DeleteNamespacedDeploymentAsync("master", "default");
                await kubeClient.GetClient().CoreV1.DeleteNamespacedConfigMapAsync("simulation-config", "default");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cleanup warning: {ex.Message}");
            }

            return resultsJson;
        }
    }
}
