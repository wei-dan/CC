using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;
using System;
using System.ClientModel;
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

// 如果需要启用 CaptureScreen 截图功能，请在 .csproj 中做如下设置：
//   <UseWindowsForms>true</UseWindowsForms>
//   并通过 NuGet 安装 System.Drawing.Common 包。
// 然后取消以下代码区域的注释，并添加相应的 using 指令。
/*
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

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
*/
