using System.Diagnostics;

namespace SimulationProject.Helper.GitCloneHelper
{
    public static class GitHelper
    {
        public static async Task<string> CloneRepoAsync(string gitUrl)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"clone {gitUrl} .",
                    WorkingDirectory = tempDir,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
                throw new Exception("Git clone failed");

            return tempDir;
        }
    }
}
