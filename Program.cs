using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

const string apiKey = "sk-ws-H.EIIMERL.YOZt.MEUCIEnJYx_DqGa8aGadlD1AzkUXKik4SYqkIaYjstnlNHpcAiEA40Q3ru1LsKM_OQ_HO32SbYCnl-M7lWgJhTWkIk3K5l0"; //"sk-c7366fcac5aa4023827e049e7a714705";

const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
const uint MOUSEEVENTF_LEFTUP   = 0x0004;

[DllImport("user32.dll")]
static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);

AIAgent agent = new OpenAIClient(
        new ApiKeyCredential(apiKey),
        new OpenAIClientOptions
        {
            Endpoint = new Uri("https://dashscope.aliyuncs.com/compatible-mode/v1")
        }
    )
    .GetChatClient("qwen3.7-plus") //("deepseek-v4-pro")
    .AsAIAgent(tools: [
        AIFunctionFactory.Create(AnalyzePicture),
        AIFunctionFactory.Create(Capture),
        AIFunctionFactory.Create(MoveMouse),
        AIFunctionFactory.Create(Click),
        AIFunctionFactory.Create(DoubleClick),
        //AIFunctionFactory.Create(CaptureRegion),
        AIFunctionFactory.Create(FindIconPosition),
        AIFunctionFactory.Create(GetMousePosition),
        AIFunctionFactory.Create(GetScreenResolution)
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

    // 获取主屏幕逻辑尺寸
    var bounds = Screen.PrimaryScreen.Bounds;

    // 考虑 DPI 缩放，确保截图覆盖完整的物理像素
    float scaleX = 1f;
    float scaleY = 1f;
    using (var devGraphics = Graphics.FromHwnd(IntPtr.Zero))
    {
        scaleX = devGraphics.DpiX / 96f;
        scaleY = devGraphics.DpiY / 96f;
    }

    int width = (int)Math.Round(bounds.Width * scaleX);
    int height = (int)Math.Round(bounds.Height * scaleY);
    int srcX = (int)Math.Round(bounds.X * scaleX);
    int srcY = (int)Math.Round(bounds.Y * scaleY);

    using var bitmap = new Bitmap(
        width,
        height,
        PixelFormat.Format32bppArgb);

    using var graphics = Graphics.FromImage(bitmap);

    // 从屏幕复制物理像素
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

//[Description("截取屏幕上指定矩形区域的图片（可用来截取你想要的图标），并保存为PNG。")]
//static string CaptureRegion(
//    [Description("截图区域左上角X坐标")] int x,
//    [Description("截图区域左上角Y坐标")] int y,
//    [Description("截图区域宽度")] int width,
//    [Description("截图区域高度")] int height,
//    [Description("要保存的文件名")] string fileName)
//{
//    // 保存到当前程序运行的目录
//    string folder = AppDomain.CurrentDomain.BaseDirectory;
//    if (!Directory.Exists(folder))
//        Directory.CreateDirectory(folder);

//    // 确保文件名以 .png 结尾
//    if (!fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
//        fileName += ".png";

//    string absolutePath = Path.Combine(folder, fileName);

//    // 获取主屏幕尺寸，修正越界坐标
//    var screenBounds = Screen.PrimaryScreen.Bounds;

//    int srcX = Math.Max(screenBounds.Left, Math.Min(screenBounds.Right, x));
//    int srcY = Math.Max(screenBounds.Top, Math.Min(screenBounds.Bottom, y));
//    int srcWidth = Math.Min(width, screenBounds.Right - srcX);
//    int srcHeight = Math.Min(height, screenBounds.Bottom - srcY);

//    if (srcWidth <= 0 || srcHeight <= 0)
//    {
//        return "指定的区域超出屏幕范围，无法截取。";
//    }

//    using var bitmap = new Bitmap(srcWidth, srcHeight, PixelFormat.Format32bppArgb);
//    using var graphics = Graphics.FromImage(bitmap);

//    graphics.CopyFromScreen(srcX, srcY, 0, 0, new Size(srcWidth, srcHeight));

//    // 可选择在此绘制很小的坐标标签便于确认（不绘制网格）
//    bitmap.Save(absolutePath, ImageFormat.Png);

//    return $"区域截图已保存：{absolutePath}";
//}

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
