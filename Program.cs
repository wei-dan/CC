using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using UIAgentLib;
using OfficeOpenXml;

const string apiKey = "sk-c7366fcac5aa4023827e049e7a714705"; // "sk-ws-H.EIIMERL.YOZt.MEUCIEnJYx_DqGa8aGadlD1AzkUXKik4SYqkIaYjstnlNHpcAiEA40Q3ru1LsKM_OQ_HO32SbYCnl-M7lWgJhTWkIk3K5l0";

const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
const uint MOUSEEVENTF_LEFTUP   = 0x0004;

[DllImport("user32.dll")]
static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);

AIAgent agent = new OpenAIClient(
        new ApiKeyCredential(apiKey),
        new OpenAIClientOptions
        {
            Endpoint = new Uri("https://api.deepseek.com") //("https://dashscope.aliyuncs.com/compatible-mode/v1")
        }
    )
    .GetChatClient("deepseek-v4-pro") //("qwen3.7-plus")
    .AsAIAgent(tools: [
        AIFunctionFactory.Create(UIAgent.GetDesktopRootInfo),
        AIFunctionFactory.Create(UIAgent.GetFocusedElementInfo),
        AIFunctionFactory.Create(UIAgent.GetElementFromPoint),
        AIFunctionFactory.Create(UIAgent.FindByAutomationId),
        AIFunctionFactory.Create(UIAgent.FindByName),
        AIFunctionFactory.Create(UIAgent.ClickByAutomationId),
        AIFunctionFactory.Create(UIAgent.ClickByName),
        AIFunctionFactory.Create(UIAgent.SetValueByAutomationId),
        AIFunctionFactory.Create(UIAgent.SetValueByName),
        AIFunctionFactory.Create(UIAgent.WaitForElementByAutomationId),
        AIFunctionFactory.Create(UIAgent.WaitForElementByName),
        AIFunctionFactory.Create(MoveMouse),
        AIFunctionFactory.Create(ExcelAgent.OpenExcelWorkbook),
        AIFunctionFactory.Create(ExcelAgent.ReadExcelCell),
        AIFunctionFactory.Create(ExcelAgent.WriteExcelCell),
        AIFunctionFactory.Create(ExcelAgent.ReadExcelRange),
        AIFunctionFactory.Create(ExcelAgent.GetExcelUsedRange),
        AIFunctionFactory.Create(ExcelAgent.AppendExcelRow),
        AIFunctionFactory.Create(ExcelAgent.CreateExcelWorkbook),
        AIFunctionFactory.Create(ExcelAgent.AddExcelSheet),
        AIFunctionFactory.Create(ExcelAgent.DeleteExcelSheet),
        AIFunctionFactory.Create(ExcelAgent.CheckExcelFileExists),
        AIFunctionFactory.Create(ExcelAgent.SetCellBackgroundColor),
        AIFunctionFactory.Create(ExcelAgent.SetCellFontColor),
        AIFunctionFactory.Create(ExcelAgent.SetCellFontBold),
        AIFunctionFactory.Create(ExcelAgent.SetCellFontItalic),
        AIFunctionFactory.Create(ExcelAgent.SetCellFontSize),
        AIFunctionFactory.Create(ExcelAgent.SetCellBorder),
        AIFunctionFactory.Create(ExcelAgent.SetColumnWidth),
        AIFunctionFactory.Create(ExcelAgent.SetRowHeight),
        AIFunctionFactory.Create(ExcelAgent.MergeCells),
        AIFunctionFactory.Create(ExcelAgent.UnmergeCells),
        AIFunctionFactory.Create(ExcelAgent.AutoFitColumns),
        AIFunctionFactory.Create(ExcelAgent.AutoFitRows)
        //AIFunctionFactory.Create(RunPowerShell)
        ]);
AgentSession session = await agent.CreateSessionAsync();

while (true)
{
    Console.Write("You: ");
    string? input = Console.ReadLine();

    if (input is null)
    {
        continue;
    }

    var response = await agent.RunAsync(input, session);

    Console.WriteLine($"Bot: { response.Text }");
}


[Description("根据提供的描述和截图路径分析图片")]
static string AnalyzePicture([Description("提供描述")]string description, [Description("截图路径")]string path)
{
    AIAgent agent1 = new OpenAIClient(
        new ApiKeyCredential(apiKey),
        new OpenAIClientOptions
        {
            Endpoint = new Uri("https://dashscope.aliyuncs.com/compatible-mode/v1")
        }
    )
    .GetChatClient("qwen3.7-plus") //("deepseek-v4-pro")
    .AsAIAgent();

    var bytes = File.ReadAllBytes(path);
    var mes = new Microsoft.Extensions.AI.ChatMessage(
        ChatRole.User,
        new AIContent[]
        {
            new TextContent(
                description
            ),
            new DataContent(
               bytes,
                "image/jpeg")
        });
    var response = agent1.RunAsync(mes).GetAwaiter().GetResult();
    return response.Text;
}

[Description("Captures the current screen and saves it as a PNG file at the specified filename.")]
static string Capture(string fileName)
{
    // 保存到当前程序运行的目录
    string folder = AppDomain.CurrentDomain.BaseDirectory;
    if (!Directory.Exists(folder))
        Directory.CreateDirectory(folder);

    // 确保文件名以 .png 结尾
    if (!fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
        fileName += ".png";

    string absolutePath = Path.Combine(folder, fileName);

    // 获取主屏幕逻辑尺寸及其在虚拟桌面中的位置
    var bounds = Screen.PrimaryScreen.Bounds;

    int width = bounds.Width;
    int height = bounds.Height;
    int srcX = bounds.X;
    int srcY = bounds.Y;

    using var bitmap = new Bitmap(
        width,
        height,
        PixelFormat.Format32bppArgb);

    using var graphics = Graphics.FromImage(bitmap);

    // 从屏幕复制像素（使用逻辑坐标，System.Windows.Forms 会自动处理桌面缩放）
    graphics.CopyFromScreen(
        srcX,
        srcY,
        0,
        0,
        new Size(width, height));

    // 保存为 PNG
    bitmap.Save(absolutePath, ImageFormat.Png);

    // 返回保存后的完整路径
    return $"full path {absolutePath}";
}

[Description("移动鼠标到屏幕上的指定坐标 (x, y)。")]
static string MoveMouse([Description("目标X像素坐标")]int x, [Description("目标Y像素坐标")]int y)
{
    Cursor.Position = new Point(x, y);
    return $"Mouse moved to ({x},{y})";
}

[Description("在当前鼠标位置执行一次左键单击，并返回执行位置。")]
static string Click()
{
    var pos = Cursor.Position;
    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
    return $"Clicked at ({pos.X},{pos.Y})";
}

[Description("在当前鼠标位置执行一次左键双击，并返回执行位置。")]
static string DoubleClick()
{
    var pos = Cursor.Position;
    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
    Thread.Sleep(100);
    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
    return $"Double-clicked at ({pos.X},{pos.Y})";
}

[Description("根据图标描述找到图标在屏幕上的像素坐标。返回格式 x,y")]
static string FindIconPosition([Description("图标描述")] string iconDescription)
{
    // 截取全屏（不添加网格），临时文件
    string tempFileName = "find_icon_" + Guid.NewGuid() + ".png";
    string folder = AppDomain.CurrentDomain.BaseDirectory;
    if (!Directory.Exists(folder))
        Directory.CreateDirectory(folder);
    string screenshotPath = Path.Combine(folder, tempFileName);

    var bounds = Screen.PrimaryScreen.Bounds;
    using (var bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb))
    using (var g = Graphics.FromImage(bmp))
    {
        g.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size);
        bmp.Save(screenshotPath, ImageFormat.Png);
    }

    // 请求模型分析图片并返回坐标
    string prompt = $"图中有一个图标，描述如下：{iconDescription}。请指出该图标中心点的屏幕像素坐标。只回复两个数字，格式：x,y（例如200,300）。如果没找到，回复 -1,-1。";

    try
    {
        string modelResponse = AnalyzePicture(prompt, screenshotPath);
        modelResponse = modelResponse.Trim();

        // 尝试解析形如“123,456”的坐标
        var match = Regex.Match(modelResponse, @"(-?\d+)\s*,\s*(-?\d+)");
        if (match.Success)
        {
            int x = int.Parse(match.Groups[1].Value);
            int y = int.Parse(match.Groups[2].Value);
            return $"{x},{y}";
        }

        return $"无法从回复中解析坐标，模型返回：{modelResponse}";
    }
    catch (Exception ex)
    {
        return $"查找图标时出错：{ex.Message}";
    }
    finally
    {
        // 删除临时文件
        try
        {
            if (File.Exists(screenshotPath))
                File.Delete(screenshotPath);
        }
        catch
        {
            // 忽略删除错误
        }
    }
}

[Description("Get current mouse cursor position")]
static string GetMousePosition()
{
    var pos = Cursor.Position;

    return $"Current mouse position: ({pos.X},{pos.Y})";
}

[Description("返回电脑主显示器的分辨率，格式为“width x height”。")]
static string GetScreenResolution()
{
    var bounds = Screen.PrimaryScreen.Bounds;
    return $"Screen resolution: {bounds.Width} x {bounds.Height}";
}

[Description("在本地 PowerShell 中运行命令并返回输出")]
static string RunPowerShell([Description("要执行的 PowerShell 命令")] string script)
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
