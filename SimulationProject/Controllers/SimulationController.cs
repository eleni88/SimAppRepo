using System.Security.Claims;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        public SimulationController(ISimulationService simulationService)
        {
            _simulationService = simulationService;
        }

        // GET /api/simulations
        [Authorize(Roles = "Admin,User")]
        [HttpGet]
        public async Task<IActionResult> GetAllSimulation()
        {
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
            return Ok(simulationsDtos);
        }

        // GET /api/simulations/{Simid}
        [Authorize(Roles = "Admin,User")]
        [HttpGet("{Simid}")]
        public async Task<IActionResult> GetSimulation(int Simid)
        {
            var simulation = await _simulationService.GetSimulationByIdAsync(Simid);
            if (simulation == null)
            {
                return BadRequest(new { message = "Simulation not found" });
            }
            var simulationDto = simulation.Adapt<SimulationDTO>();
            return Ok(simulationDto);
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
        [HttpPost("{Simid}")]
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

    }

}
