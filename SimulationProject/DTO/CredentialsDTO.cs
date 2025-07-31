using SimulationProject.DTO.ProviderDTOs;

namespace SimulationProject.DTO
{
    public class CredentialsDTO
    {
        public int Credid { get; set; }
        public string Accesskey { get; set; } = "";
        public string Secretkey { get; set; } = "";
        public string Subscriptionid { get; set; } = "";
        public string Clientid { get; set; } = "";
        public string Clientsecret { get; set; } = "";
        public string Tenantid { get; set; } = "";
        public ProviderDTO Provider { get; set; }
    }
}
