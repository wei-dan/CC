using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

const string apiKey = "sk-ws-H.EIIMERL.YOZt.MEUCIEnJYx_DqGa8aGadlD1AzkUXKik4SYqkIaYjstnlNHpcAiEA40Q3ru1LsKM_OQ_HO32SbYCnl-M7lWgJhTWkIk3K5l0"; //"sk-c7366fcac5aa4023827e049e7a714705";
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
        AIFunctionFactory.Create(Capture)
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

    var mes = new Microsoft.Extensions.AI.ChatMessage(
        ChatRole.User,
        new AIContent[]
        {
            new TextContent(
                description
            ),
            new UriContent(
                new Uri(path),
                "image/jpeg")
        });
    var response = agent1.RunAsync(mes).GetAwaiter().GetResult();
    return response.Text;
}

[Description("Captures the current screen and saves it as a PNG file at the specified path.")]
static string Capture(string filePath)
{
    // 获取主屏幕尺寸
    var bounds = Screen.PrimaryScreen.Bounds;

    using var bitmap = new Bitmap(
        bounds.Width,
        bounds.Height,
        PixelFormat.Format32bppArgb);

    using var graphics = Graphics.FromImage(bitmap);

    // 从屏幕复制像素
    graphics.CopyFromScreen(
        bounds.X,
        bounds.Y,
        0,
        0,
        bounds.Size);

    // 保存
    bitmap.Save(filePath, ImageFormat.Png);

    return $"Screenshot saved to {filePath}";
}