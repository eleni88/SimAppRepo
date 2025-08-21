using k8s.KubeConfigModels;
using Microsoft.EntityFrameworkCore;
using SimulationProject.Data;
using SimulationProject.Models;

namespace SimulationProject.Services
{
    public class CloudCredentialsService
    {
        private readonly SimSaasContext _context;
        private readonly IPasswordHashService _passwordHashService;

        public CloudCredentialsService(SimSaasContext context, IPasswordHashService passwordHashService)
        {
            _context = context;
            _passwordHashService = passwordHashService;
        }

        // find user's credentials
        public async Task<Cloudcredential> GetCredsByUseridasync(int userid)
        {
            return await _context.Cloudcredentials.FirstOrDefaultAsync(creds => creds.Userid == userid);
        }

        // find credentials by credid
        public async Task<Cloudcredential> GetCredsByCredIdAsync(int credid)
        {
            return await _context.Cloudcredentials.FindAsync(credid);
        }

        // Add credentials
        public async Task AddCredentials(Cloudcredential cloudcredential)
        {
            _context.Cloudcredentials.Add(cloudcredential);
            await _context.SaveChangesAsync();
        }

        //update credentials
        public async Task UpdateCloudCreadentialsAsync()
        {
            await _context.SaveChangesAsync();
        }

        // hash credentials
        public async Task<string> HashCredentialsAsync(string newcreds, string oldcreds)
        {
            string newcredshash = "";
            bool hashverified = false;
            if (!string.IsNullOrEmpty(oldcreds))
            {
                hashverified = _passwordHashService.VerifyUserPassword(newcreds, oldcreds);
            }

            if (!hashverified)
            {
                newcredshash = _passwordHashService.HashUserPassword(newcreds);
            }
            return newcredshash;
        }
    }
}
