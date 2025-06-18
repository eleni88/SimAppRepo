using k8s;

namespace SimulationProject.Services
{
    public class PollingService
    {
        private readonly ILogger<SimulationService> _logger;

        public PollingService(ILogger<SimulationService> logger){
            _logger = logger;
        }
        public async Task WaitForSimulationToFinishAsync(IKubernetes client, string labelSelector, int expectedSlaves)
        {
            const int pollingIntervalSeconds = 10;
            const string namespaceName = "default";

            _logger.LogInformation("Waiting for slave pods to complete...");

            while (true)
            {
                var pods = await client.CoreV1.ListNamespacedPodAsync(
                    namespaceParameter: namespaceName,
                    labelSelector: labelSelector
                );

                var completed = pods.Items.Count(p =>
                    p.Status.Phase == "Succeeded" || p.Status.Phase == "Completed"
                );

                _logger.LogInformation("Completed pods: {Completed}/{Expected}", completed, expectedSlaves);

                if (completed >= expectedSlaves)
                    break;

                await Task.Delay(TimeSpan.FromSeconds(pollingIntervalSeconds));
            }
        }
    }
}
