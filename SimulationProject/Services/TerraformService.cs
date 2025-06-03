using SimulationProject.Helper.TerraformHelper;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace SimulationProject.Services
{
    public class TerraformService
    {
        private readonly string _workingDirectory;

        public TerraformService(string workingDirectory)
        {
            _workingDirectory = workingDirectory;
        }

        public async Task<TerraformResult> InitAsync()
        {
            return await RunCommandAsync("terraform", "init");
        }

        public async Task<TerraformResult> ApplyAsync()
        {
            return await RunCommandAsync("terraform", "apply -auto-approve");
        }

        public async Task<TerraformResult> DestroyAsync()
        {
            return await RunCommandAsync("terraform", "destroy -auto-approve");
        }

        private async Task<TerraformResult> RunCommandAsync(string fileName, string arguments)
        {
            var result = new TerraformResult();
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            var processStartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = _workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processStartInfo };

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    outputBuilder.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    errorBuilder.AppendLine(e.Data);
            };

            try
            {
                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync();

                result.Success = process.ExitCode == 0;
                result.Output = outputBuilder.ToString();
                result.Error = errorBuilder.ToString();
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.ToString();
            }

            return result;
        }

        public async Task<string> GetKubeConfigAsync()
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "terraform",
                Arguments = "output -json",
                WorkingDirectory = _workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using var proc = Process.Start(processStartInfo);
            var stdout = await proc.StandardOutput.ReadToEndAsync();
            var stderr = await proc.StandardError.ReadToEndAsync();
            await proc.WaitForExitAsync();

            if (proc.ExitCode != 0)
            {
                throw new Exception($"Terraform output failed: {stderr}");
            }

            using var doc = JsonDocument.Parse(stdout);
            if (!doc.RootElement.TryGetProperty("kubeconfig", out var kubeElement))
            {
                throw new Exception("kubeconfig output not found in Terraform JSON output.");
            }
            if (!kubeElement.TryGetProperty("value", out var valueElement))
            {
                throw new Exception("kubeconfig.value not found in Terraform JSON output.");
            }

            return valueElement.GetString() ?? throw new Exception("Kubeconfig not found in Terraform output.");
        }


    }
}
