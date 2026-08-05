using CC.Agents.LinuxAgents;
using CommunityToolkit.VectorData.SqliteVec;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace CC.Agents.PowerShellAgents;

public static class PowerShellAgent
{
    public static AIAgent CreatePowerShellAgent()
    {
        const string apiKey = "sk-c7366fcac5aa4023827e049e7a714705";

        TextSearchProviderOptions textSearchOptions = new()
        {
            SearchTime = TextSearchProviderOptions.TextSearchBehavior.BeforeAIInvoke,
        };
        static async Task<IEnumerable<TextSearchProvider.TextSearchResult>> SearchAdapter(string query, CancellationToken cancellationToken)
        {
            var results = new List<TextSearchProvider.TextSearchResult>();
            return results;
        }

        AIAgent agent = new OpenAIClient(
                new ApiKeyCredential(apiKey),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri("https://api.deepseek.com")
                }
            )
            .GetChatClient("deepseek-v4-pro")
            .AsAIAgent(new ChatClientAgentOptions
            {
                Name = "PowerShellAgent",
                ChatOptions = new()
                {
                    Tools =
                    [
                        AIFunctionFactory.Create(PowerShellAgentFunctions.RunPowerShell)
                    ]
                },
                AIContextProviders = [new TextSearchProvider(SearchAdapter, textSearchOptions)]
            });
        return agent;
    }
}