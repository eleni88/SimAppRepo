using System.Diagnostics;
using System.Text;
using System.Text.Json;
using k8s;
using k8s.Models;

namespace SimulationProject.Helper.KubernetesHelper
{
    public class KubernetesDeployerHelper
    {
        private readonly IKubernetes _client;

        // Creation of KubernetesClient
        public KubernetesDeployerHelper(string kubeConfig)
        {
            var config = KubernetesClientConfiguration.BuildConfigFromConfigFile(
                new MemoryStream(Encoding.UTF8.GetBytes(kubeConfig)));
            _client = new Kubernetes(config);
        }

        public async Task DeployYamlFilesAsync(List<string> yamlFiles)
        {
            foreach (var file in yamlFiles)
            {
                var content = await File.ReadAllTextAsync(file);
                var resources = KubernetesYamlHelper.LoadAllObjects(content);

                foreach (var resource in resources)
                {
                    switch (resource)
                    {
                        case V1Deployment deployment:
                            await _client.AppsV1.CreateNamespacedDeploymentAsync(deployment, deployment.Metadata.NamespaceProperty ?? "default");
                            break;
                        case V1Service service:
                            await _client.CoreV1.CreateNamespacedServiceAsync(service, service.Metadata.NamespaceProperty ?? "default");
                            break;
                        case V1ConfigMap configMap:
                            await _client.CoreV1.CreateNamespacedConfigMapAsync(configMap, configMap.Metadata.NamespaceProperty ?? "default");
                            break;
                        default:
                            Console.WriteLine($"Unsupported resource type: {resource.GetType().Name}");
                            break;
                    }
                }
            }
        }
        public IKubernetes GetClient() => _client;
    }
}
