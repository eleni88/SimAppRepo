using System.Security.Claims;
using k8s.KubeConfigModels;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimulationProject.DTO.SimExecutionDTOs;
using SimulationProject.DTO.SimulationDTOs;
using SimulationProject.DTO.UserDTOs;
using SimulationProject.Models;
using SimulationProject.Services;

namespace SimulationProject.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SimulationRunController : ControllerBase
    {
        private readonly ISimulationService _simulationService;
        private readonly MinikubeDeployService _minikubeDeployService;
        private readonly SimulationRunService _simulationRunService;
        private readonly IUsersService _usersService;

        public SimulationRunController(ISimulationService simulationService, MinikubeDeployService minikubeDeployService, SimulationRunService simulationRunService, IUsersService usersService)
        {
            _simulationService = simulationService;
            _minikubeDeployService = minikubeDeployService;
            _simulationRunService = simulationRunService;
            _usersService = usersService;
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
                await _simulationService.PutSimuExecutionAsync();

                return Ok(new { message = "Simulation succeeded" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Simulation failed: {ex.Message}");
            }
        }
        //--------------------- Run Simulation in Cloud Provider -----------------------
        [HttpPost("run")]
        public async Task<IActionResult> RunCloudSimulation([FromBody] SimulationProviderRunDTO request)
        {
            //extract user from token
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                return BadRequest(new { message = "Unauthorized user" });
            }
            var userId = Int32.Parse(userIdStr);
            var user = await _usersService.GetUserByIdAsync(userId);
            if (user is null)
            {
                return NotFound(new { message = "User Not Found" });
            }
            var userdto = user.Adapt<UserDto>();

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

                string resultsJson = await _simulationRunService.RunSimulationAsync(
                    request.Codeurl,
                    request.Simparams,
                    request.SimcloudNavigation.Cloudid,
                    request.Region.Regioncode,
                    request.Resourcerequirement.Instancetype,
                    request.Resourcerequirement.Mininstances,
                    request.Resourcerequirement.Maxinstances,
                    userdto,
                    newsimexec
                );
                if (resultsJson == null)
                {
                    return BadRequest(new { message = "Simulation failed" } );
                }
                newsimexec.Enddate = DateTime.UtcNow;
                TimeSpan duration = (TimeSpan)(newsimexec.Enddate - newsimexec.Startdate);
                newsimexec.Duration = duration.ToString(@"hh\:mm\:ss");
                await _simulationService.CreateSimExecutionAsync(newsimexec);
                return Ok(new { message = "Simulation succeeded" });

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Simulation failed: {ex.Message}");
            }
        }

    }
}
