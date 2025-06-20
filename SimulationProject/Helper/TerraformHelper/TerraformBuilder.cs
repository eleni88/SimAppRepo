using System.Text;

namespace SimulationProject.Helper.TerraformHelper
{
    public class TerraformBuilder
    {
        private readonly StringBuilder _tfBuilder = new();
        private string _workingDir = "/tmp/terraform";

        public TerraformBuilder UseWorkingDirectory(string path)
        {
            _workingDir = path;
            Directory.CreateDirectory(_workingDir);
            return this;
        }

        public TerraformBuilder AddRequiredProviders()
        {
            _tfBuilder.AppendLine(@"
                                    terraform {
                                      required_providers {
                                        aws = {
                                          source  = ""hashicorp/aws""
                                          version = ""~> 5.0""
                                        }
                                      }
                                    }
            ");
            return this;
        }

        public TerraformBuilder AddAwsProvider(string region, string accessKey, string secretKey)
        {
            _tfBuilder.AppendLine($@"
                                    provider ""aws"" {{
                                      region     = ""{region}""
                                      access_key = ""{accessKey}""
                                      secret_key = ""{secretKey}""
                                    }}
                                    ");
            return this;
        }

        public TerraformBuilder AddEksCluster(string clusterName, string region, int desired, int min, int max)
        {
            //--------------------------------------------
            // Use of Terraform Registry modules vpc and eks
            // First vpc in order to create vpc where the eks cluster will be created in
            _tfBuilder.AppendLine($@" 
                                  module ""vpc"" {{
                                      source = ""terraform-aws-modules/vpc/aws""    

                                      name = ""my-vpc""
                                      cidr = ""10.0.0.0/16""

                                      azs = [""{region}a"", ""{region}b"", ""{region}c""]
                                      private_subnets = [""10.0.1.0/24"", ""10.0.2.0/24"", ""10.0.3.0/24""]
                                      public_subnets = [""10.0.101.0/24"", ""10.0.102.0/24"", ""10.0.103.0/24""]

                                      enable_nat_gateway = true
                                      enable_vpn_gateway = true

                                      tags = {{
                                                Terraform = ""true""
                                                Environment = ""dev""
                                      }}
                                                }}

                                  module ""eks"" {{
                                      source  = ""terraform-aws-modules/eks/aws""
                                      version = ""~> 20.31""
                                      cluster_name    = ""{clusterName}""
                                      cluster_version = ""1.31""

                                      enable_cluster_creator_admin_permissions = true

                                      cluster_compute_config = {{
                                          enabled    = true
                                          node_pools = [""general-purpose""]
                                          }}

                                      vpc_id     = module.vpc.vpc_id
                                      subnet_ids = module.vpc.private_subnets

                                      tags = {{
                                        Environment = ""dev""
                                        Terraform   = ""true""
                                      }}

                                     eks_managed_node_groups = {{
                                        default = {{
                                          ami_type       = ""AL2023_x86_64_STANDARD""
                                          instance_types = [""t3.medium""]

                                          min_size     = {min}
                                          max_size     = {max}
                                          desired_size = {desired}
                                        }}
                                      }} 
                                    }}

                                    output ""kubeconfig"" {{
                                        description = ""Generated kubeconfig file""
                                        value = yamlencode({{
                                        apiVersion = ""v1""
                                        kind       = ""Config""
                                        clusters = [{{
                                            name = module.eks.cluster_name
                                            cluster = {{
                                            server = module.eks.cluster_endpoint
                                            certificate-authority-data = module.eks.cluster_certificate_authority_data
                                            }}
                                        }}]
                                        contexts = [{{
                                            name = ""eks-context""
                                            context = {{
                                            cluster = module.eks.cluster_name
                                            user    = ""aws""
                                            }}
                                        }}]
                                        current-context = ""eks-context""
                                        users = [{{
                                            name = ""aws""
                                            user = {{
                                            exec = {{
                                                apiVersion = ""client.authentication.k8s.io/v1beta1""
                                                command    = ""aws""
                                                args       = [
                                                ""eks"",
                                                ""get-token"",
                                                ""--region"",
                                                {region},
                                                ""--cluster-name"",
                                                module.eks.cluster_name
                                                ]
                                            }}
                                            }}
                                        }}]
                                        }})
                                        sensitive = true
                                    }}
");

            //-------------------------------------------------------------

            return this;
        }

        public async Task<string> CreateTerraformFile()
        {
            var tfPath = Path.Combine(_workingDir, "main.tf");
            await File.WriteAllTextAsync(tfPath, _tfBuilder.ToString());
            return tfPath;
        }
    }
}
