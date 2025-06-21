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
    }
}
