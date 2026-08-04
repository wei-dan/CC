using System.ComponentModel;
using System.Diagnostics;

public static class LinuxAgent
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

    [Description("Get the current Debian version information (lsb_release -a).")]
    public static string GetDebianVersion()
    {
        return RunLinuxCommand("lsb_release -a");
    }

    [Description("Update the APT package list (runs 'apt-get update').")]
    public static string UpdatePackageList()
    {
        return RunLinuxCommand("apt-get update -qq");
    }

    [Description("Install a package via APT (apt-get install -y).")]
    public static string InstallPackage(
        [Description("Name of the package to install.")] string packageName)
    {
        return RunLinuxCommand($"apt-get install -y {packageName}");
    }
}
