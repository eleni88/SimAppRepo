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
