using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

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

    if (input == "/screenshot")
    {
        var imageBytes = CaptureScreen();
        File.WriteAllBytes("screenshot.png", imageBytes);
        Console.WriteLine("Bot: 截图已保存为 screenshot.png");
        continue;
    }

    var response = await agent.RunAsync(input, session);
    Console.WriteLine($"Bot: { response.Text }");
}

// 请确保在 .csproj 中引用 System.Drawing.Common 包并设置 <UseWindowsForms>true</UseWindowsForms>
static byte[] CaptureScreen()
{
    var bounds = Screen.PrimaryScreen.Bounds;
    using var bitmap = new Bitmap(bounds.Width, bounds.Height);
    using var graphics = Graphics.FromImage(bitmap);
    graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size);
    using var ms = new MemoryStream();
    bitmap.Save(ms, ImageFormat.Png);
    return ms.ToArray();
}
