using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimulationProject.DTO.SimExecutionDTOs;
using SimulationProject.DTO.SimulationDTOs;
using SimulationProject.Models;
using SimulationProject.Services;

namespace SimulationProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SimulationRunController : ControllerBase
    {
        private readonly ISimulationService _simulationService;
        private readonly PollingService _PollingService;
        private readonly MinikubeDeployService _minikubeDeployService;

        public SimulationRunController(ISimulationService simulationService, PollingService pollingService, MinikubeDeployService minikubeDeployService)
        {
            _simulationService = simulationService;
            _PollingService = pollingService;
            _minikubeDeployService = minikubeDeployService;
        }

        [HttpPost("minikube/run")]
        public async Task<IActionResult> RunSimulation([FromBody] SimulationDTO request)
        {
            var simulation = await _simulationService.GetSimulationByIdAsync(request.Simid);
            if (simulation == null)
            {
                return BadRequest(new { message = "Simulation not found" });
            }
            try
            {
                // create new simExecution
                var newsimexec = new Simexecution
                {
                    Simid = request.Simid,
                    Startdate = DateTime.UtcNow
                };
                await _simulationService.CreateSimExecutionAsync(newsimexec);

                string resultsJson = await _minikubeDeployService.RunSimulationToMinikubeAsync(
                    request.Codeurl,
                    request.Simparams,
                    newsimexec
                );
                newsimexec.Enddate = DateTime.UtcNow;
                TimeSpan duration = (TimeSpan)(newsimexec.Enddate - newsimexec.Startdate);
                newsimexec.Duration = duration.ToString(@"hh\:mm\:ss");
                await _simulationService.CreateSimExecutionAsync(newsimexec);

                return Ok(new { message = "Simulation succeed" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Simulation failed: {ex.Message}");
            }
        }
    }
}
