using k8s;
using System.Text;

namespace SimulationProject.Helper.KubernetesHelper
{
    public static class KubeConfigValidator
    {
        public static void ValidateKubeConfig(string kubeConfigContent)
        {
            if (string.IsNullOrWhiteSpace(kubeConfigContent))
                throw new Exception("Kubeconfig is empty.");

            try
            {
                var configStream = new MemoryStream(Encoding.UTF8.GetBytes(kubeConfigContent));
                var config = KubernetesClientConfiguration.BuildConfigFromConfigFile(configStream);

                if (config == null || string.IsNullOrEmpty(config.Host))
                    throw new Exception("Invalid kubeconfig: Missing cluster host.");

                if (config.CurrentContext == null)
                    throw new Exception("Invalid kubeconfig: Missing current context.");

                if (config.Namespace == null)
                    Console.WriteLine("Warning: kubeconfig does not specify a namespace (default will be used).");
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to parse or validate kubeconfig content: " + ex.Message);
            }
        }
    }
}
