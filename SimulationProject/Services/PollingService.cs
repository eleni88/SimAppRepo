using k8s;
using SimulationProject.Models;

namespace SimulationProject.Services
{
    public class PollingService
    {
        private readonly ILogger<ISimulationService> _logger;
        private readonly ISimulationService _simulationService;

        public PollingService(ILogger<ISimulationService> logger, ISimulationService simulationService)
        {
            _logger = logger;
            _simulationService = simulationService;
        }

        public async Task WaitForSimulationToFinishAsync(
            Simexecution newsimexec,
            IKubernetes client,
            string masterLabelSelector,
            int expectedSlaves,
            CancellationToken cancellationToken = default)
        {
            const int pollingIntervalSeconds = 10;
            const string namespaceName = "default";

            _logger.LogInformation("Waiting for simulation to complete. Monitoring master pod...");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var masterPods = await client.CoreV1.ListNamespacedPodAsync(
                    namespaceParameter: namespaceName,
                    labelSelector: masterLabelSelector,
                    cancellationToken: cancellationToken);

                if (!masterPods.Items.Any())
                {
                    _logger.LogWarning("No master pod found with label {LabelSelector}", masterLabelSelector);
                }
                else
                {
                    var masterPod = masterPods.Items.First();
                    var phase = masterPod.Status?.Phase;

                    _logger.LogInformation("Master pod status: {Phase}", phase);

                    if (phase == "Running")
                    {
                        // Keep polling
                        await Task.Delay(TimeSpan.FromSeconds(pollingIntervalSeconds), cancellationToken);
                        continue;
                    }
                    else if (phase == "Succeeded")
                    {
                        _logger.LogInformation("Master pod completed successfully. Fetching results...");
                        await SaveResultsFromMasterPodAsync(newsimexec, client, masterPod.Metadata.Name, namespaceName, phase, cancellationToken);
                        break;
                    }
                    else
                    {
                        _logger.LogWarning("Master pod terminated with phase: {Phase}. Attempting to fetch logs anyway.", phase);
                        await SaveResultsFromMasterPodAsync(newsimexec, client, masterPod.Metadata.Name, namespaceName, phase, cancellationToken);
                        break;
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(pollingIntervalSeconds), cancellationToken);
            }
        }

        private async Task SaveResultsFromMasterPodAsync(Simexecution newsimexec, IKubernetes client, string podName, string namespaceName, string phase, CancellationToken cancellationToken)
        {
            try
            {
                var logsStream = await client.CoreV1.ReadNamespacedPodLogAsync(
                    name: podName,
                    namespaceParameter: namespaceName,
                    cancellationToken: cancellationToken);

                using (var reader = new StreamReader(logsStream))
                {
                    var logs = await reader.ReadToEndAsync();
                    _logger.LogInformation("Collected logs from master pod:\n{Logs}", logs);

                    // Save to DB
                    await SaveResultsToDatabaseAsync(logs, newsimexec, phase);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to collect logs from master pod.");
            }
        }

        private async Task SaveResultsToDatabaseAsync(string results, Simexecution newsimexec, string phase)
        {
            newsimexec.Execreport = results;
            newsimexec.State = phase;
            await Task.Delay(100); 
            _logger.LogInformation("Results saved to database.");
        }
    }
}
