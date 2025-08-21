using SimulationProject.DTO.ProviderDTOs;

namespace SimulationProject.DTO.CredentialsDTOs
{
    public class CredentialsDTO
    {
        public int Userid { get; set; }
        public int Credid { get; set; }
        public string Accesskeyid { get; set; } = "";
        public string Secretaccesskey { get; set; } = "";
        public string Subscriptionid { get; set; } = "";
        public string Clientid { get; set; } = "";
        public string Clientsecret { get; set; } = "";
        public string Tenantid { get; set; } = "";
        public string Gcpservicekeyjson { get; set; } = "";
        public string Gcpprojectid { get; set; } = "";
        public int Cloudid { get; set; } 
    }
}
