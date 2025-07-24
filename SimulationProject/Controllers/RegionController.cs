using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimulationProject.Services;

namespace SimulationProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegionController : ControllerBase
    {
        private readonly RegionService _regionService;

        public RegionController(RegionService regionService)
        {
            _regionService = regionService;
        }

        // GET /api/region
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllProviders()
        {
            var providers = await _regionService.GetAllRegionsAsync();
            return Ok(providers);
        }
    }
}
