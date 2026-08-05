using System.ComponentModel;
using System.Diagnostics;

namespace CC.Agents.LinuxAgents;

public static class LinuxAgentFunctions
{
    [Description("Execute an arbitrary shell command in a Debian Linux environment and return its standard output.")]
    public static string RunLinuxCommand(
        [Description("The shell command to execute (e.g., 'ls -la').")] string command)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{command.Replace("\"", "\\\"")}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            return string.IsNullOrEmpty(error) ? output : $"{output}\nError: {error}";
        }
        catch (Exception ex)
        {
            return $"Exception: {ex.Message}";
        }
    }
}
