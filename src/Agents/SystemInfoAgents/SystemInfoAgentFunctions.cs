using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace CC.Agents.SystemInfoAgents;

public static class SystemInfoAgentFunctions
{
    /// <summary>
    /// 获取当前电脑的系统信息
    /// </summary>
    /// <returns>格式化的系统信息字符串</returns>
    [Description("获取当前电脑的系统信息")]
    public static string GetSystemInfo()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"操作系统: {RuntimeInformation.OSDescription}");
        sb.AppendLine($"系统架构: {RuntimeInformation.OSArchitecture}");
        sb.AppendLine($"处理器架构: {RuntimeInformation.ProcessArchitecture}");
        sb.AppendLine($"机器名称: {Environment.MachineName}");
        sb.AppendLine($"用户名称: {Environment.UserName}");
        sb.AppendLine($"当前目录: {Environment.CurrentDirectory}");
        sb.AppendLine($".NET 版本: {Environment.Version}");
        sb.AppendLine($"系统已运行时间: {Environment.TickCount / 1000} 秒");
        return sb.ToString();
    }
}
