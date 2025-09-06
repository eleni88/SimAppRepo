using System.Text;
using k8s.KubeConfigModels;

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

        public TerraformBuilder AddRequiredProviders(int Provider)
        {
            if (Provider == 1) {
                _tfBuilder.AppendLine(@"
                                    terraform {
                                      required_providers {
                                        aws = {
                                          source  = ""hashicorp/aws""
                                          version = ""~> 6.0""
                                        }
                                        kubernetes = {
                                            source  = ""hashicorp/kubernetes""
                                            version = "">= 2.20.0""
                                        }
                                        helm = {
                                            source  = ""hashicorp/helm""
                                            version = "">= 2.10.0""
                                        }
                                      }
                                    }
                                ");
            }
            else
            if (Provider == 2)
            {
                _tfBuilder.AppendLine(@"
                                    terraform {
                                      required_providers {

                                        kubernetes = {
                                          source  = ""hashicorp/kubernetes""
                                          version = "">= 2.0.3""
                                        }

                                        google = {
                                          source  = ""hashicorp/google""
                                          version = ""~> 6.0""
                                        }

                                        helm = {
                                              source  = ""hashicorp/helm""
                                              version = "">= 2.1.0""
                                        }
                                      }
                                    }
                    ");
            }
            else
            if (Provider == 3)
            {
                _tfBuilder.AppendLine(@"
                                    terraform {
                                      required_providers {

                                         kubernetes = {
                                          source  = ""hashicorp/kubernetes""
                                          version = "">= 2.0.3""
                                        }

                                         azurerm = {
                                          source = ""hashicorp/azurerm""
                                          version = ""~> 4.0""
                                        }

                                        helm = {
                                          source  = ""hashicorp/helm""
                                          version = "">= 2.1.0""
                                        }
                                      }
                                    }
                                ");
            }

                return this;
        }
        //------------------ AWS Provider -----------------------------
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

        //------------------ AZURE Provider -----------------------------
        public TerraformBuilder AddAzureProvider(string region, string subscriptionId, string clientId, string clientSecret, string tenantId)
        {
            _tfBuilder.AppendLine($@"
                                    provider ""azurerm"" {{
                                     resource_provider_registrations = ""none""
                                      features {{}}
                                      subscription_id = ""{subscriptionId}""
                                      client_id       = ""{clientId}""
                                      client_secret   = ""{clientSecret}""
                                      tenant_id       = ""{tenantId}""
                                    }}

                                    
                                ");
            return this;
        }

        //------------------ GOOGLE Provider -----------------------------
        public TerraformBuilder AddGoogleProvider(string gcpprojectId, string region, string gcpservicekeyjson)
        {
            _tfBuilder.AppendLine($@"
                                    provider ""google"" {{
                                    project = ""{gcpprojectId}""
                                    region  = ""{region}""
                                    zone    = ""{region}-c""
                                    credentials = ""{gcpservicekeyjson}""
                                }}

                                                                      
                                ");

            return this;
        }


        //------------------ AWS Cluster (Using AWS Module) -----------------------------
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

                                    provider ""kubernetes"" {{
                                      host                   = data.aws_eks_cluster.eks.endpoint
                                      cluster_ca_certificate = base64decode(data.aws_eks_cluster.eks.certificate_authority[0].data)
                                      token                  = data.aws_eks_cluster_auth.eks.token
                                    }}
                             
                                    provider ""helm"" {{
                                      kubernetes {{
                                        host                   = data.aws_eks_cluster.eks.endpoint
                                        cluster_ca_certificate = base64decode(data.aws_eks_cluster.eks.certificate_authority[0].data)
                                        token                  = data.aws_eks_cluster_auth.eks.token
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

            return this;
        }

        //------------------ AZURE Cluster (Official Provider) -----------------------------
        public TerraformBuilder AddAzureCluster(string clusterName, string region, int desired, int min, int max)
        {
            _tfBuilder.AppendLine($@" 
                                resource  ""azurerm_resource_group"" ""aks"" {{
                                  name     = ""aks-rg""
                                  location = {region}
                                }}

                                resource ""azurerm_virtual_network"" ""aks_vnet"" {{
                                  name                = ""aks-vnet""
                                  location            = azurerm_resource_group.aks.location
                                  resource_group_name = azurerm_resource_group.aks.name
                                  address_space       = [""10.0.0.0/16""]
                                }}

                                resource ""azurerm_subnet"" ""aks_subnet"" {{
                                  name                 = ""aks-subnet""
                                  resource_group_name  = azurerm_resource_group.aks.name
                                  virtual_network_name = azurerm_virtual_network.aks_vnet.name
                                  address_prefixes     = [""10.0.1.0/24""]
                                }}
                                
                        
                                resource ""azurerm_kubernetes_cluster"" ""aks"" {{
                                  name                = {clusterName}
                                  location            = azurerm_resource_group.aks.location
                                  resource_group_name = azurerm_resource_group.aks.name
                                  dns_prefix          = ""akscluster""

                                  default_node_pool {{
                                    name                = ""general""
                                    vm_size             = ""Standard_D2_v2""
                                    enable_auto_scaling = true
                                    min_count           = {min}
                                    max_count           = {max}
                                    node_count          = {desired}
                                    vnet_subnet_id      = azurerm_subnet.aks_subnet.id
                                  }}

                                 provider ""kubernetes"" {{
                                      host                   = data.azurerm_kubernetes_cluster.aks.kube_config.0.host
                                      client_certificate     = base64decode(data.azurerm_kubernetes_cluster.aks.kube_config.0.client_certificate)
                                      client_key             = base64decode(data.azurerm_kubernetes_cluster.aks.kube_config.0.client_key)
                                      cluster_ca_certificate = base64decode(data.azurerm_kubernetes_cluster.aks.kube_config.0.cluster_ca_certificate)
                                    }}

                                    provider ""helm"" {{
                                      kubernetes {{
                                        host                   = data.azurerm_kubernetes_cluster.aks.kube_config.0.host
                                        client_certificate     = base64decode(data.azurerm_kubernetes_cluster.aks.kube_config.0.client_certificate)
                                        client_key             = base64decode(data.azurerm_kubernetes_cluster.aks.kube_config.0.client_key)
                                        cluster_ca_certificate = base64decode(data.azurerm_kubernetes_cluster.aks.kube_config.0.cluster_ca_certificate)
                                      }}
                                    }}

                                identity {{type = ""SystemAssigned""}}

                                tags = {{
                                        Environment = ""dev""
                                        Terraform   = ""true""
                                      }}

                                output ""kubeconfig"" {{
                                  value     = azurerm_kubernetes_cluster.aks.kube_config_raw
                                  sensitive = true
                                }}

            ");

            return this;
        }

        public TerraformBuilder AddGkeCluster(string clusterName, string region, string gcpprojectId, int desired, int min, int max)
        {
            _tfBuilder.AppendLine($@"                                  
                                    resource ""google_compute_network"" ""vpc"" {{
                                      name                    = ""{gcpprojectId}-vpc""
                                      auto_create_subnetworks = ""false""
                                    }}

                                    # Subnet
                                    resource ""google_compute_subnetwork"" ""subnet"" {{
                                      name          = ""{gcpprojectId}-subnet""
                                      region        = ""{region}""
                                      network       = google_compute_network.vpc.name
                                      ip_cidr_range = ""10.10.0.0/24""
                                    }}

                                    data ""google_container_engine_versions"" ""gke_version"" {{
                                      location = ""{region}""
                                      version_prefix = ""1.27.""
                                    }}

                                    resource ""google_container_cluster"" ""gke"" {{
                                      name     = ""{clusterName}""
                                      location = ""{region}""
                                      remove_default_node_pool = true
                                      initial_node_count       = 1

                                      network    = google_compute_network.vpc.name
                                      subnetwork = google_compute_subnetwork.subnet.name
                                    }}


                                    resource ""google_container_node_pool"" ""gke_nodes"" {{
                                      name       = google_container_cluster.gke.name
                                      location   = ""{region}""
                                      cluster    = google_container_cluster.gke.name
  
                                      version = data.google_container_engine_versions.gke_version.release_channel_default_version[""STABLE""]
                                      node_count = ""{desired}""
                                    autoscaling {{
                                        min_node_count = {min}
                                        max_node_count = {max}
                                      }}

                                    node_config {{
                                                oauth_scopes = [
                                                  ""https://www.googleapis.com/auth/logging.write"",
                                                  ""https://www.googleapis.com/auth/monitoring"",
                                                ]

                                                labels = {{
                                                  environment = ""dev""
                                                  terraform   = ""true""
                                                }}

                                                machine_type = ""e2-standard-2""
                                                tags         = [""gke-node"", ""${gcpprojectId}-gke""]
                                                metadata = {{
                                                  disable-legacy-endpoints = ""true""
                                                }}
                                              }}
                
                                output ""kubernetes_cluster_host"" {{
                                          value       = google_container_cluster.gke.endpoint
                                          description = ""GKE Cluster Host""
                                }}
                              
                               data ""google_client_config"" ""gke"" {{}}

                                provider ""kubernetes"" {{
                                      host  = ""https://${{data.google_container_cluster.gke.endpoint}}""
                                      token = data.google_client_config.gke.access_token
                                      cluster_ca_certificate = base64decode(
                                        data.google_container_cluster.gke.master_auth[0].cluster_ca_certificate,
                                      )
                                    }}

                                    provider ""helm"" {{
                                      kubernetes {{
                                        host  = ""https://${{data.google_container_cluster.gke.endpoint}}""
                                        token = data.google_client_config.gke.access_token
                                        cluster_ca_certificate = base64decode(
                                          data.google_container_cluster.gke.master_auth[0].cluster_ca_certificate,
                                        )
                                      }}
                                    }}

                               output kubeconfig {{
                                    description = ""Generated kubeconfig file""
                                     value = yamlencode{{
                                        apiVersion  ""v1""
                                        kind = ""Config""
                                        current-context = ""context-gke""
                                        preferences = {{}}
                                        clusters = [
                                          cluster = {{
                                            certificate-authority-data = google_container_cluster.gke_cluster.master_auth[0].cluster_ca_certificate
                                            server = ""https://${{google_container_cluster.gke.endpoint}}""
                                          }}
                                          name = google_container_cluster.name
                                        ]
                                        contexts = [
                                          context = {{
                                            cluster = google_container_cluster.gke.name
                                            user = ""gke""
                                            }}
                                          name = ""context-gke""                                            
                                          ]
                                        users = [
                                          name = ""gke""
                                          user = {{
                                            auth-provider = {{
                                              name   = ""gcp""
                                              config = {{
                                                access-token = data.google_client_config.gke.access_token
                                                cmd-path     = ""gcloud""
                                                cmd-args     = ""config config-helper --format=json""
                                                expiry       = data.google_client_config.gke.token_expiry
                                                token-type   = ""Bearer""
                                              }}
                                            }}
                                          }}
                                        ]
                                    }}               
                                }}
                            ");
            return this;

        }

        //public TerraformBuilder AddKubernetesMetrics()
        //{
        //    _tfBuilder.AppendLine($@"
        //        resource ""helm_release"" ""metrics_server"" {{
        //          name       = ""metrics-server""
        //          chart      = ""metrics-server""
        //          repository = ""https://kubernetes-sigs.github.io/metrics-server/""
        //          namespace  = ""kube-system""
        //          version    = ""3.13.0""
        //        }}

                

        //    ");
        //}

        public async Task<string> CreateTerraformFile()
        {
            var tfPath = Path.Combine(_workingDir, "main.tf");
            await File.WriteAllTextAsync(tfPath, _tfBuilder.ToString());
            return tfPath;
        }
    }
}
