using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

string apiKey = "sk-c7366fcac5aa4023827e049e7a714705";
AIAgent agent = new OpenAIClient(
        new ApiKeyCredential(apiKey),
        new OpenAIClientOptions
        {
            Endpoint = new Uri("https://api.deepseek.com")
        })
        .GetChatClient("deepseek-v4-pro")
        .AsAIAgent(
            tools: [

            ]);
AgentSession session = await agent.CreateSessionAsync();
while (true)
{
    Console.Write("You: ");
    string? input = Console.ReadLine();

    // 当输入流结束（EOF）时退出循环
    if (input is null)
    {
        break;
    }
    var response = await agent.RunAsync(input, session);
    Console.WriteLine($"Bot: { response.Text }");
}

/// <summary>
/// 对当前电脑主屏幕进行截图，并将图像数据保存为 PNG 格式的字节数组。
/// </summary>
/// <returns>PNG 图像的字节数组</returns>
static byte[] CaptureScreen()
{
    // 获取主屏幕的边界
    var bounds = Screen.PrimaryScreen.Bounds;
    using var bitmap = new Bitmap(bounds.Width, bounds.Height);
    using var graphics = Graphics.FromImage(bitmap);
    graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size);
    using var ms = new MemoryStream();
    bitmap.Save(ms, ImageFormat.Png);
    return ms.ToArray();
}
