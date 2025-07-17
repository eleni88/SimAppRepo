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

                if ((content.Contains("master") && content.Contains("Deployment")) ||
                    (content.Contains("master") && content.Contains("Service")))// || (content.Contains("slave") && content.Contains("Pod")))
                {
                    var dest = Path.Combine(tempDir, Path.GetFileName(yamlFile));
                    File.Copy(yamlFile, dest, overwrite: true);
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

                var input = new StringReader(content);
                var yamlStream = new YamlStream();
                yamlStream.Load(input);

                foreach (var document in yamlStream.Documents)
                {
                    if (document.RootNode is not YamlMappingNode rootNode)
                        continue;

                    string kind = null;
                    string name = null;
                    string role = null;

                    // Try to read kind
                    if (rootNode.Children.TryGetValue(new YamlScalarNode("kind"), out var kindNode))
                    {
                        kind = kindNode.ToString();
                    }

                    // Try to read metadata.name
                    if (rootNode.Children.TryGetValue(new YamlScalarNode("metadata"), out var metadataNode)
                        && metadataNode is YamlMappingNode metadataMap)
                    {
                        if (metadataMap.Children.TryGetValue(new YamlScalarNode("name"), out var nameNode))
                        {
                            name = nameNode.ToString();
                        }

                        // Try to read metadata.labels.role (or app if needed)
                        if (metadataMap.Children.TryGetValue(new YamlScalarNode("labels"), out var labelsNode)
                            && labelsNode is YamlMappingNode labelsMap)
                        {
                            if (labelsMap.Children.TryGetValue(new YamlScalarNode("role"), out var roleNode))
                            {
                                role = roleNode.ToString();
                            }
                        }
                    }

                    // Robust detection
                    if ((role?.Contains("master") ?? false) || (name?.Contains("master") ?? false))
                    {
                        if (kind == "Deployment")
                            result.HasMaster = true;
                    }

                    if ((role?.Contains("slave") ?? false) || (name?.Contains("slave") ?? false))
                    {
                        if (kind == "Pod")
                            result.SlaveCount++;
                    }
                }
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

            var specNode = (YamlMappingNode)root["spec"];

            var templateNode = (YamlMappingNode)specNode["template"]; 
            
            var podSpec = (YamlMappingNode)templateNode["spec"];

            // Add volumes
            if (!podSpec.Children.TryGetValue("volumes", out var volumesNode))
            {
                volumesNode = new YamlSequenceNode();
                podSpec.Add("volumes", volumesNode);
            }

            var volumesSeq = (YamlSequenceNode)volumesNode;

            var existingVolume = volumesSeq.Children
                .OfType<YamlMappingNode>()
                .FirstOrDefault(v => v.Children.TryGetValue("name", out var n) && n.ToString() == "config-volume");

            if (existingVolume == null)
            {
                var newVolume = new YamlMappingNode
                {
                    { "name", "config-volume" },
                    { "configMap", new YamlMappingNode { { "name", "simulation-config" } } }
                };
                volumesSeq.Add(newVolume);
            }
            else
            {
                existingVolume.Children["configMap"] = new YamlMappingNode { { "name", "simulation-config" } };
            }

            // Add volumeMounts to first container
            var containers = (YamlSequenceNode)podSpec["containers"];
            var firstContainer = (YamlMappingNode)containers.Children[0];

            if (!firstContainer.Children.TryGetValue("volumeMounts", out var volumeMountsNode))
            {
                volumeMountsNode = new YamlSequenceNode();
                firstContainer.Add("volumeMounts", volumeMountsNode);
            }

            var volumeMountsSeq = (YamlSequenceNode)volumeMountsNode;

            var existingMount = volumeMountsSeq.Children
                .OfType<YamlMappingNode>()
                .FirstOrDefault(m => m.Children.TryGetValue("mountPath", out var p) && p.ToString() == "/app/config");

            if (existingMount != null)
            {
                // overwrite to point to config-volume
                existingMount.Children["name"] = new YamlScalarNode("config-volume");
            }
            else
            {
                var newMount = new YamlMappingNode
                {
                    { "name", "config-volume" },
                    { "mountPath", "/app/config" }
                };
                volumeMountsSeq.Add(newMount);
            }

            // Save back to file
            using var writer = new StringWriter();
            yamlStream.Save(writer);
            File.WriteAllText(yamlPath, writer.ToString());
        }
    }
}
