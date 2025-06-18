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

        public TerraformBuilder AddEksCluster(string clusterName, string vpcId, string[] subnetIds, int desired, int min, int max)
        {
            var subnetList = string.Join(", ", subnetIds.Select(id => $"\"{id}\""));
            _tfBuilder.AppendLine($@"
                                    resource ""aws_eks_cluster"" ""main"" {{
                                        name     = ""{clusterName}""
                                        role_arn = aws_iam_role.eks_cluster.arn

                                        vpc_config {{
                                        subnet_ids = [{subnetList}]
                                        }}
                                    }}

                                    output ""kubeconfig"" {{
                                        value = aws_eks_cluster.main.kubeconfig[0]
                                        sensitive = true
                                    }}
                                    ");
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
