using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;
using System;
using System.ClientModel;

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
