using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace SimulationProject.Helper.KubernetesHelper
{
    public class YamlParseResult
    {
        public bool HasMaster { get; set; }
        public int SlaveCount { get; set; }
    }
    public class YamlHelper
    {
        public static List<string> FindYamlFiles(string repoPath)
        {
            return Directory.GetFiles(repoPath, "*.yaml", SearchOption.AllDirectories).ToList();
        }

        public static List<string> CopyYamlFilesToTmp(List<string> yamlFiles, string tempDir)
        {
            // Copy only needed YAMLs
            foreach (var yamlFile in yamlFiles)
            {
                var content = File.ReadAllText(yamlFile);

                if ((content.Contains("master") && content.Contains("Deployment")) || (content.Contains("slave") && content.Contains("Pod")))
                {
                    var dest = Path.Combine(tempDir, Path.GetFileName(yamlFile));
                    File.Copy(yamlFile, dest);
                }
            }
            return Directory.GetFiles(tempDir, "*.yaml", SearchOption.AllDirectories).ToList();
        }

        // parse Yaml files to check if they are master/slave
        public static YamlParseResult ParseYamlFiles(List<string> files)
        {
            var result = new YamlParseResult();
            foreach (var file in files)
            {
                var content = File.ReadAllText(file);
                if (content.Contains("master") && content.Contains("Deployment"))
                    result.HasMaster = true;
                if (content.Contains("slave") && content.Contains("Pod"))
                    result.SlaveCount++;
            }
            return result;
        }

        public static void AddConfigMapToDeploymentYaml(string yamlPath)
        {
            var yaml = File.ReadAllText(yamlPath);

            // Load YAML
            var input = new StringReader(yaml);
            var yamlStream = new YamlStream();
            yamlStream.Load(input);

            var root = (YamlMappingNode)yamlStream.Documents[0].RootNode;

            var specNode = root["spec"] as YamlMappingNode;
            
            var templateNode = specNode["template"] as YamlMappingNode;
            
            var podSpec = templateNode["spec"] as YamlMappingNode;

            // Add volumes
            if (!podSpec.Children.ContainsKey("volumes"))
            {
                var volumesNode = new YamlSequenceNode
            {
                new YamlMappingNode
                {
                    { "name", "config-volume" },
                    {
                        "configMap", new YamlMappingNode
                        {
                            { "name", "simulation-config" }
                        }
                    }
                }
            };
                podSpec.Add("volumes", volumesNode);
            }

            // Add volumeMounts to first container
            var containers = podSpec["containers"] as YamlSequenceNode;
            var firstContainer = containers.Children[0] as YamlMappingNode;

            if (!firstContainer.Children.ContainsKey("volumeMounts"))
            {
                var volumeMounts = new YamlSequenceNode
            {
                new YamlMappingNode
                {
                    { "name", "config-volume" },
                    { "mountPath", "/app/config" }
                }
            };
                firstContainer.Add("volumeMounts", volumeMounts);
            }

            // Save back to file
            using var writer = new StringWriter();
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            yamlStream.Save(writer);
            File.WriteAllText(yamlPath, writer.ToString());
        }
    }
}
