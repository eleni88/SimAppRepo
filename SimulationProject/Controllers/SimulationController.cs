using System.Security.Claims;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimulationProject.DTO.SimExecutionDTOs;
using SimulationProject.DTO.SimulationDTOs;
using SimulationProject.DTO.UserDTOs;
using SimulationProject.Helper.HateoasHelper;
using SimulationProject.Models;
using SimulationProject.Services;

namespace SimulationProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SimulationController : ControllerBase
    {
        private readonly ISimulationService _simulationService;
        private readonly IUsersService _usersService;
        private readonly ILinkService<SimulationDTO> _linkService;

        public SimulationController(ISimulationService simulationService, IUsersService usersService, ILinkService<SimulationDTO> linkService)
        {
            _simulationService = simulationService;
            _usersService = usersService;
            _linkService = linkService;
        }

        // GET /api/simulations
        [Authorize(Roles = "Admin,User")]
        [HttpGet]
        public async Task<IActionResult> GetAllSimulation()
        {
            string baseUri = $"{Request.Scheme}://{Request.Host}";

            //extract user and role from token
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                return BadRequest(new { message = "Unauthorized user" });
            }
            var userId = Int32.Parse(userIdStr);
            var userRoleStr = User.FindFirstValue(ClaimTypes.Role);
            var simulations = await _simulationService.GetAllSimulationsAsync();
            var usersSimulations = simulations;
            var simulationsWithLinks = new List<LinkResponseWrapper<SimulationDTO>>();
            
            if ((simulations != null) && (userRoleStr == "User"))
            {
                if (simulations.Any(simulation => simulation.Simuser == userId))
                {
                    usersSimulations = simulations.Where(simulation => simulation.Simuser == userId);
                    var simulationsDtos = usersSimulations.Select(simulation => simulation.Adapt<SimulationDTO>());
                    simulationsWithLinks = _linkService.AddLinksToList(simulationsDtos, baseUri);
                }
            }
            else
            if ((simulations != null) && (userRoleStr == "Admin"))
            {
                var simulationsDtos = usersSimulations.Select(simulation => simulation.Adapt<SimulationDTO>());
                simulationsWithLinks = _linkService.AddLinksToList(simulationsDtos, baseUri);
            }

                return Ok(simulationsWithLinks);
        }

        // GET /api/simulations/{Simid}
        [Authorize(Roles = "Admin,User")]
        [HttpGet("{Simid}")]
        public async Task<IActionResult> GetSimulation(int Simid)
        {
            string baseUri = $"{Request.Scheme}://{Request.Host}";

            var simulation = await _simulationService.GetSimulationByIdAsync(Simid);
            if (simulation == null)
            {
                return BadRequest(new { message = "Simulation not found" });
            }
            var simulationDto = simulation.Adapt<SimulationDTO>();
            var simulationWithLinks = _linkService.AddLinks(simulationDto, baseUri);
            return Ok(simulationWithLinks);
        }

        // POST /api/simulations/create
        [Authorize(Roles = "Admin,User")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateSimulationAsync([FromBody] CreateSimulationDTO createSimulationDTO)
        {
            //extract user and role from token
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                return BadRequest(new { message = "Unauthorized user" });
            }
            var userId = Int32.Parse(userIdStr);
            var simulation = createSimulationDTO.Adapt<Simulation>();
            if (simulation == null)
            {
                return BadRequest();
            }
            simulation.Simuser = userId;
            simulation.Createdate = DateTime.UtcNow;
            await _simulationService.CreateSimulationAsync(simulation);
            return CreatedAtAction(nameof(GetSimulation), new { Simid = simulation.Simid }, new { Simid = simulation.Simid });
        }

        //PUT /api/simulation/{Simid}
        [Authorize(Roles = "Admin,User")]
        [HttpPut("{Simid}")]
        public async Task<IActionResult> UpdateSimulation(int Simid, [FromBody] UpdateSimulationDTO updateSimulationDTO)
        {
            var simulation = await _simulationService.GetSimulationByIdAsync(Simid);
            if (simulation == null)
            {
                return NotFound(new { message = "Simulation not found" });
            }
            var simexecutions = simulation.Simexecutions;
            if ((simexecutions != null) && (simexecutions.Any(simexc => simexc.State == "Running")))
            {
                return BadRequest(new { message = "Simulation is running. It cannot be modified." });
            }
            updateSimulationDTO.Adapt(simulation);
            int rowsaffected = await _simulationService.PutSimulationAsync();
            if (rowsaffected > 0)
            {
                return Ok(new { message = "Simulation updated successfully" });
            }
            else
            {
                return BadRequest(new { message = "Update failed" });
            }
        }

        // DELETE /api/simulation/{Simid}
        [Authorize(Roles = "Admin,User")]
        [HttpDelete("{Simid}")]
        public async Task<IActionResult> DeleteSimulation(int Simid)
        {
            var simulation = await _simulationService.GetSimulationByIdAsync(Simid);
            if (simulation == null)
            {
                return NotFound(new { message = "Simulation not found" });
            }
            var simexecutions = simulation.Simexecutions;
            if ((simexecutions != null) && (simexecutions.Any(simexc => simexc.State == "Running")))
            {
                return BadRequest(new { message = "Simulation is running. It cannot be deleted." });
            }
            await _simulationService.DeleteSimulationAsync(simulation);
            return NoContent();
        }

        //------------------ Simulation Executions ------------------------

        // GET /api/{Simid}/simexecutions/{Execid}
        [Authorize(Roles = "Admin,User")]
        [HttpGet("{Simid}/simexecutions/{Execid}")]
        public async Task<ActionResult<SimExecutionDTO>> GetSimulationSimExecution(int Simid, int Execid)
        {
            var simulation = await _simulationService.GetSimulationByIdAsync(Simid);
            if (simulation == null)
            {
                return NotFound(new { message = "Simulation not found" });
            }
            var simexecution = await _simulationService.GetSimulationSimExecutionAsync(Simid, Execid);

            if (simexecution == null)
                return NotFound(new { message = "Execution not found"});

            var simexecDto = simexecution.Adapt<SimExecutionDTO>();
            return Ok(simexecDto);
        }

        // DELETE /api/{Simid}/simexecutions/{Execid}
        [Authorize(Roles = "Admin,User")]
        [HttpDelete("{Simid}/simexecutions/{Execid}")]
        public async Task<IActionResult> DeleteSimulationSimExecution(int Simid, int Execid)
        {
            var simulation = await _simulationService.GetSimulationByIdAsync(Simid);
            if (simulation == null)
            {
                return NotFound(new { message = "Simulation not found" });
            }
            var simexecution = await _simulationService.GetSimulationSimExecutionAsync(Simid, Execid);

            if (simexecution == null)
                return NotFound(new { message = "Execution not found" });

            if (simexecution.State == "Running")
            {
                return BadRequest(new { message = "Execution is running. It cannot be deleted." });
            }

            await _simulationService.DeleteSimulationSimExecutionAsync(simexecution);

            return NoContent();
        }

        //--------------------- Get Simulation's Results -------------------------------
        // DELETE /api/{Simid}/simexecutions/{Execid}/results
        [HttpGet("{Simid}/simexecutions/{Execid}/results")]
        public async Task<ActionResult<String>> GetResults(int Simid, int Execid)
        {
            string ResultsStr = "";
            var simexec = await _simulationService.GetSimulationSimExecutionAsync(Simid, Execid);

            if (simexec == null)
            {
                return NotFound(new { message = "Simulation execution not found." });
            }
            ResultsStr = simexec.Execreport;
            if (String.IsNullOrEmpty(ResultsStr))
            {
                return BadRequest(new { message = "There are no results." });
            }
            return ResultsStr;
        }
    }

}
