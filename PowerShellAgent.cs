using System.ComponentModel;
using System.Diagnostics;

public static class PowerShellAgent
{
    [Description("在本地 PowerShell 中运行命令并返回输出")]
    public static string RunPowerShell([Description("要执行的 PowerShell 命令")] string script)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -NonInteractive -Command \"{script}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
        };

        using var process = Process.Start(psi);
        string stdout = process!.StandardOutput.ReadToEnd();
        string stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (!string.IsNullOrWhiteSpace(stderr))
            return $"Error: {stderr}{Environment.NewLine}{stdout}";

        return stdout;
    }
}
