using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace CC.Agents.SingBoxAgents;

public static class SingBoxAgent
{
    public static AIAgent CreateSingBoxAgent()
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
                Name = "SingBoxAgent",
                ChatOptions = new()
                {
                    Instructions = "你是一个智能助手，负责安装sing-box, 下载rule-set文件, 生成sing-box配置文件。",
                    Tools =
                    [
                        AIFunctionFactory.Create(SingBoxFunctions.GetSingBoxConfig),
                        AIFunctionFactory.Create(SingBoxFunctions.InstallSingBox),
                        AIFunctionFactory.Create(SingBoxFunctions.DownloadRuleSets),
                    ]
                },
                AIContextProviders = [new TextSearchProvider(SearchAdapter, textSearchOptions)]
            });
        return agent;
    }
}