namespace SimulationProject.DTO.CredentialsDTOs
{
    public class UpdateCredentialsDTO
    {
        public int Credid { get; set; }
        public int Userid { get; set; }
        public int Cloudid { get; set; }
        public string Accesskeyid { get; set; } = "";
        public string SecretAccesskey { get; set; } = "";
        public string Subscriptionid { get; set; } = "";
        public string Clientid { get; set; } = "";
        public string Clientsecret { get; set; } = "";
        public string Tenantid { get; set; } = "";
        public string Gcpservicekeyjson { get; set; } = "";
        public string Gcpprojectid { get; set; } = "";
    }
}
