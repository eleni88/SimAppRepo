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

        //------------- Simulation Execution ------------------
        Task<Simexecution> GetSimulationSimExecutionAsync(int Simid, int Execid);
        Task DeleteSimulationSimExecutionAsync(Simexecution simexecution);
        Task<int> PutSimuExecutionAsync();
        Task CreateSimExecutionAsync(Simexecution simexecution);
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
            var valid = false;
            if (codeurl != null)
            {
                Uri.TryCreate(codeurl, UriKind.Absolute, out Uri uriResult);
                valid = (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            }
            return valid;

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
            return await _context.Simulations.Include(sim => sim.Simexecutions)
                .Include(sim => sim.Region)
                .Include(sim => sim.SimcloudNavigation)
                .ToListAsync();
        }

        //get by id
        public async Task<Simulation> GetSimulationByIdAsync(int Simulation)
        {
            return await _context.Simulations.Include(sim => sim.Simexecutions)
                .Include(sim => sim.Region)
                .Include(sim => sim.SimcloudNavigation)
                .Include(sim => sim.Resourcerequirements)
                .FirstOrDefaultAsync(sim => sim.Simid == Simulation);
        }

        // get by name
        public async Task<Simulation> GetSimulationByNameAsync(string Simname)
        {
            return await _context.Simulations.Include(sim => sim.Simexecutions)
                .Include(sim => sim.Region)
                .Include(sim => sim.SimcloudNavigation)
                .Include(sim => sim.Resourcerequirements)
                .FirstOrDefaultAsync(sim => sim.Name == Simname);
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

        //------------------------ Simulation Execution ----------------------
        //get by simid and execid
        public async Task<Simexecution> GetSimulationSimExecutionAsync(int Simid, int Execid)
        {
            return await _context.Simexecutions.Where(s => s.Simid == Simid && s.Execid == Execid).FirstOrDefaultAsync();
        }

        //delete simexecution
        public async Task DeleteSimulationSimExecutionAsync(Simexecution simexecution)
        {
            _context.Simexecutions.Remove(simexecution);
            await _context.SaveChangesAsync();
        }

        //put
        public async Task<int> PutSimuExecutionAsync()
        {
            int rowsAfected = 0;
            rowsAfected = await _context.SaveChangesAsync();
            return rowsAfected;
        }

        // post  
        public async Task CreateSimExecutionAsync(Simexecution simexecution)
        {
            _context.Simexecutions.Add(simexecution);
            await _context.SaveChangesAsync();
        }
    }
}
