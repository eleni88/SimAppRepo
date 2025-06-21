using k8s.Models;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace SimulationProject.Helper.KubernetesHelper
{
    public static class ConfigMapGenerator
    {
        public static async Task<string> GenerateConfigMapFileAsync(string jsonContent, string outputDir)
        {
            var configMap = new V1ConfigMap
            {
                ApiVersion = "v1",
                Kind = "ConfigMap",
                Metadata = new V1ObjectMeta { Name = "simulation-config" },
                Data = new Dictionary<string, string> { { "params.json", jsonContent } }
            };

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yaml = serializer.Serialize(configMap);
            var path = Path.Combine(outputDir, "configmap.yaml");
            await File.WriteAllTextAsync(path, yaml);
            return path;
        }
    }
}
