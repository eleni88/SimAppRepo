using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SimulationProject.Data;
using SimulationProject.Models;

namespace SimulationProject.Services
{
    public interface ISimulationService
    {
        Task<IEnumerable<Simulation>> GetAllSimulationsAsync();
        Task<Simulation> GetSimulationByIdAsync(int Simulation);
        Task<Simulation> GetSimulationByNameAsync(string Simname);
        Task CreateSimulationAsync(Simulation simulation);
        Task<int> PutSimulationAsync();
        Task DeleteSimulationAsync(Simulation simulation);
        bool IsValidUrl(string codeurl);
        bool IsValidJson(string simparamsjson);
    }
    public class SimulationService: ISimulationService
    {
        private readonly SimSaasContext _context;

        public SimulationService(SimSaasContext context)
        {
            _context = context;
        }

        //CodeURL Check
        public bool IsValidUrl(string codeurl)
        {
            Uri.TryCreate(codeurl, UriKind.Absolute, out Uri uriResult);
            return (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        //Json Check
        public bool IsValidJson(string simparamsjson)
        {
            if (string.IsNullOrWhiteSpace(simparamsjson))
                return false;

            try
            {
                JsonDocument.Parse(simparamsjson);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        // get
        public async Task<IEnumerable<Simulation>> GetAllSimulationsAsync()
        {
            return await _context.Simulations.ToListAsync();
        }

        //get by id
        public async Task<Simulation> GetSimulationByIdAsync(int Simulation)
        {
            return await _context.Simulations.FindAsync(Simulation);
        }

        // get by name
        public async Task<Simulation> GetSimulationByNameAsync(string Simname)
        {
            return await _context.Simulations.FirstOrDefaultAsync(u => u.Name == Simname);
        }

        //post
        public async Task CreateSimulationAsync(Simulation simulation)
        {
            _context.Simulations.Add(simulation);
            await _context.SaveChangesAsync();
        }

        //put
        public async Task<int> PutSimulationAsync()
        {
            int rowsAfected = 0;
            rowsAfected = await _context.SaveChangesAsync();
            return rowsAfected;
        }

        //delete
        public async Task DeleteSimulationAsync(Simulation simulation)
        {
            _context.Simulations.Remove(simulation);
            await _context.SaveChangesAsync();
        }
    }
}
