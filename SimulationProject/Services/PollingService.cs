using System.Net;
using k8s;
using k8s.Autorest;
using SimulationProject.DTO.SimExecutionDTOs;
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
            const int pollingIntervalSeconds = 5;
            const string namespaceName = "default";
            string resultsJson = "";
            var address = "";

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
                        // Fetch results from master service
                        try
                        {
                            var httpClient = new HttpClient();

                            //------- for localhost test and debug with port-forward --------------
                            //var response = await httpClient.GetAsync("http://localhost:30080/results", cancellationToken); //  NodePort 
                            //--------------------------------------------

                            // when the Web API runs on minikube too -------------------
                            //var response = await httpClient.GetAsync("http://master/results");
                            var response = await httpClient.GetAsync("http://master.default.svc.cluster.local/results");
                            //--------------------------------------------
                            if (response.IsSuccessStatusCode)
                            {
                                resultsJson = await response.Content.ReadAsStringAsync();
                                _logger.LogInformation("Simulation Results (JSON):\n{Results}", resultsJson);
                            }
                            else
                            {
                                _logger.LogWarning("Failed to fetch results. Status: {Status}", response.StatusCode);
                                resultsJson = $"Error: Failed to fetch results. Status {response.StatusCode}";
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error retrieving results from master service");
                            resultsJson = $"Exception: {ex.Message}";                       
                        }
                        // Keep polling
                        await Task.Delay(TimeSpan.FromSeconds(pollingIntervalSeconds), cancellationToken);
                        continue;
                    }
                    else
                    if (phase == "Succeeded")
                    {
                        _logger.LogInformation("Master pod completed successfully. Fetching results...");
                        await SaveResultsFromMasterPodAsync(newsimexec, client, masterPod.Metadata.Name, namespaceName, phase, resultsJson, cancellationToken);
                        break;
                    }
                    else
                    if (phase != "Pending")
                    {
                        _logger.LogWarning("Master pod terminated with phase: {Phase}. Attempting to fetch logs anyway.", phase);
                        await SaveResultsFromMasterPodAsync(newsimexec, client, masterPod.Metadata.Name, namespaceName, phase, resultsJson, cancellationToken);
                        break;
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(pollingIntervalSeconds), cancellationToken);
            }
        }

        private async Task SaveResultsFromMasterPodAsync(Simexecution newsimexec, IKubernetes client, string podName, string namespaceName, string phase, string jsonresults, CancellationToken cancellationToken)
        {
            string logresults = string.Empty;
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

                    if (jsonresults != null)
                    {
                        logresults = jsonresults;
                    }
                    else
                    {
                        logresults = logs;
                    }
                    // Save to DB
                    if (logresults != null)
                    {
                        await SaveResultsToDatabaseAsync(logresults, newsimexec, phase);
                    }
                }
            }
            catch (HttpOperationException ex)
            {
                var status = ex.Response?.StatusCode;
                var content = ex.Response?.Content;

                _logger.LogError(ex,
                    "K8s API error while collecting logs from pod {PodName}. Status: {Status}. Response: {Content}",
                    podName, status, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to collect logs from master pod.");
            }
        }

        private async Task SaveResultsToDatabaseAsync(string results, Simexecution newsimexec, string phase)
        {
            if (results != null)
            {
                newsimexec.Execreport = results;
                newsimexec.State = phase;
                await Task.Delay(100);
                _logger.LogInformation("Results saved to database.");
            }
            else
            {
                _logger.LogInformation("Results are empty.");
            }
        }
    }
}
