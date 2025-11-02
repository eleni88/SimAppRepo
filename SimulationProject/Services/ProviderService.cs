using Microsoft.EntityFrameworkCore;
using SimulationProject.Data;
using SimulationProject.Models;

namespace SimulationProject.Services
{
    public class ProviderService
    {
        private readonly SimSaasContext _context;

        public ProviderService(SimSaasContext context)
        {
            _context = context;
        }

        // get
        public async Task<IEnumerable<Cloudprovider>> GetAllProvidersAsync()
        {
            return await _context.Cloudproviders.Include(p => p.Regions)
                                                .Include(i => i.Instancetypes).ToListAsync();
        }
    }
}
