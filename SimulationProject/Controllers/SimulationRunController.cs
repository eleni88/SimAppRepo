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
        private readonly MinikubeDeployService _minikubeDeployService;

        public SimulationRunController(ISimulationService simulationService, MinikubeDeployService minikubeDeployService)
        {
            _simulationService = simulationService;
            _minikubeDeployService = minikubeDeployService;
        }

        //------------------------ Minikube test ----------------------------
        [HttpPost("minikube/run")]
        public async Task<IActionResult> RunSimulation([FromBody] SimulationRunDTO request)
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
                if (resultsJson == null)
                {
                    return BadRequest("Simulation failed");
                }
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
        //--------------------- Run Simulation in Cloud Provider -----------------------




    }
}
