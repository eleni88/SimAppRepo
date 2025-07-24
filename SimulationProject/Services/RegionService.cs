using Microsoft.EntityFrameworkCore;
using SimulationProject.Data;
using SimulationProject.Models;

namespace SimulationProject.Services
{
    public class RegionService
    {
        private readonly SimSaasContext _context;

        public RegionService(SimSaasContext context)
        {
            _context = context;
        }

        // get
        public async Task<IEnumerable<Region>> GetAllRegionsAsync()
        {
            return await _context.Regions.ToListAsync();
        }
    }
}
