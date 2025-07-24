using System.Security.Claims;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimulationProject.Services;

namespace SimulationProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProviderController : ControllerBase
    {

        private readonly ProviderService _providerService;

        public ProviderController(ProviderService providerService)
        {
            _providerService = providerService;
        }

        // GET /api/provider
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllProviders()
        {
            var providers = await _providerService.GetAllProvidersAsync();
            return Ok(providers);
        }
    }
}
