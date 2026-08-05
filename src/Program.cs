using CC.Agents;
using CC.Agents.ExcelAgents;
using CommunityToolkit.VectorData.SqliteVec;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.ComponentModel;

var agent = MainAgent.CreateMainAgent();

AgentSession session = await agent.CreateSessionAsync();

while (true)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("You: ");
    string? input = Console.ReadLine();
    Console.ResetColor();

    if (input is null)
    {
        continue;
    }

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("Bot: ");

    int windowWidth = Console.WindowWidth;
    int lineLength = 5; // "Bot: " 的长度

    await foreach (var update in agent.RunStreamingAsync(input, session))
    {
        if (string.IsNullOrEmpty(update?.Text))
        {
            continue;
        }

        foreach (char ch in update.Text)
        {
            if (ch == '\n')
            {
                Console.WriteLine();
                lineLength = 0;
                continue;
            }

            Console.Write(ch);
            lineLength++;

            if (lineLength >= windowWidth)
            {
                Console.WriteLine();
                lineLength = 0;
            }
        }
    }

    Console.ResetColor();
    Console.WriteLine();
}
