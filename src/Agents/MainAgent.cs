using CC.Agents.ExcelAgents;
using CC.Agents.LinuxAgents;
using CC.Agents.PowerShellAgents;
using CommunityToolkit.VectorData.SqliteVec;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace CC.Agents;

public static class MainAgent
{
    public static AIAgent CreateMainAgent()
    {
        const string apiKey = "sk-c7366fcac5aa4023827e049e7a714705";

        TextSearchProviderOptions textSearchOptions = new()
        {
            SearchTime = TextSearchProviderOptions.TextSearchBehavior.BeforeAIInvoke,
        };
        static async Task<IEnumerable<TextSearchProvider.TextSearchResult>> SearchAdapter(string query, CancellationToken cancellationToken)
        {
            var store = new SqliteVectorStore("Data Source=vector.db");
            var chunkCollection = store.GetCollection<int, DocumentChunk>("skhotels");
            var results = new List<TextSearchProvider.TextSearchResult>();

            var queryVector = default(ReadOnlyMemory<float>);
            try
            {
                var queryEmbedder = new OllamaEmbeddingGenerator(
                    new Uri("http://localhost:11434"),
                    "qwen3-embedding"
                );
                var queryEmbedding = await queryEmbedder.GenerateAsync(query);
                queryVector = queryEmbedding.Vector;
            }
            catch (Exception)
            {
                // 如果无法访问 Ollama 模型，直接返回空结果
                return results;
            }

            await using var searchEnumerator = chunkCollection.SearchAsync(queryVector, 3).GetAsyncEnumerator(cancellationToken);
            while (await searchEnumerator.MoveNextAsync())
            {
                var hit = searchEnumerator.Current;
                if (hit.Score > 0.3)
                {
                    continue;
                }
                results.Add(new TextSearchProvider.TextSearchResult
                {
                    Text = hit.Record?.Content ?? string.Empty
                });
            }

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
                ChatOptions = new()
                {
                    Instructions = "",
                    Tools =
                    [
                        ExcelAgent.CreateExcelAgent().AsAIFunction(),
                        LinuxAgent.CreateLinuxAgent().AsAIFunction(),
                        PowerShellAgent.CreatePowerShellAgent().AsAIFunction()
                    ]
                },
                AIContextProviders = [new TextSearchProvider(SearchAdapter, textSearchOptions)]
            });

            return agent;
    }
}