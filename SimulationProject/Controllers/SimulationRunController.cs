using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimulationProject.DTO.SimulationDTOs;
using SimulationProject.Helper.KubernetesHelper;
using SimulationProject.Services;

namespace SimulationProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SimulationRunController : ControllerBase
    {
        private readonly ISimulationService _simulationService;

        public SimulationRunController(ISimulationService simulationService)
        {
            _simulationService = simulationService;
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
                string resultsJson = await MinikubeDeployHelper.RunSimulationToMinikubeAsync(
                    request.Codeurl,
                    request.Simparams
                );

                return Content(resultsJson, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Simulation failed: {ex.Message}");
            }
        }
    }
}
