using System.Security.Claims;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimulationProject.DTO.SimExecutionDTOs;
using SimulationProject.DTO.SimulationDTOs;
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
            if ((simulations != null) && (userRoleStr == "User"))
            {
                 usersSimulations = (IEnumerable<Simulation>)simulations.Select(simulation => simulation.Simuser == userId);
            }

            var simulationsDtos = usersSimulations.Select(simulation => simulation.Adapt<SimulationDTO>());
            var simulationsWithLinks = _linkService.AddLinksToList(simulationsDtos, baseUri);
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
            await _simulationService.CreateSimulationAsync(simulation);
            return CreatedAtAction(nameof(GetSimulation), new { Simid = simulation.Simid }, simulation);
        }

        //PUT /api/simulations/{Simid}
        [Authorize(Roles = "Admin,User")]
        [HttpPut("{Simid}")]
        public async Task<IActionResult> UpdateSimulation(int Simid, [FromBody] UpdateSimulationDTO updateSimulationDTO)
        {
            var simulation = await _simulationService.GetSimulationByIdAsync(Simid);
            if (simulation == null)
            {
                return NotFound(new { message = "Simulation not found" });
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

        // DELETE /api/simulations/{Simid}
        [Authorize(Roles = "Admin,User")]
        [HttpDelete("{Simid}")]
        public async Task<IActionResult> DeleteSimulation(int Simid)
        {
            var simulation = await _simulationService.GetSimulationByIdAsync(Simid);
            if (simulation == null)
            {
                return NotFound(new { message = "Simulation not found" });
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

            var saleDto = simexecution.Adapt<SimExecutionDTO>();
            return Ok(saleDto);
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

            if (simexecution.State == "ongoing")
            {
                return BadRequest(new { message = "Execution is ongoing. It cannot be deleted" });
            }

            await _simulationService.DeleteSimulationSimExecutionAsync(simexecution);

            return NoContent();
        }
    }

}
