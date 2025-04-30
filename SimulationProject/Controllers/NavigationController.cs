using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimulationProject.DTO.UserDTOs;
using SimulationProject.Services;

namespace SimulationProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NavigationController : ControllerBase
    {
        private readonly LinkService _linkService;
        public NavigationController(LinkService linkService)
        {
            _linkService = linkService;
        }

        [HttpGet]
        public IActionResult GetNavigationLinks()
        {
            string baseUri = $"{Request.Scheme}://{Request.Host}";

            var isAuth = User.Identity?.IsAuthenticated ?? false;
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            var links = _linkService.AddAuthorizedLinks(baseUri, isAuth, role);

            return Ok(links);
        }
    }
}
