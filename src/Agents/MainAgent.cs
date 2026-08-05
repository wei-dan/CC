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
                    Instructions = """
                        你是一个功能强大的智能助手，能够通过调用子代理来处理多种任务。
                        你可以处理 Excel 表格（打开、读取、写入、设置样式、合并单元格等）、
                        执行 PowerShell 命令、执行 Linux 命令。

                        当用户提出与表格操作相关的需求时，你应该调用 Excel 子代理来完成；当用户要求执行 PowerShell 脚本时，调用 PowerShell 子代理；当用户要求执行 Linux 命令或操作 Linux 环境时，调用 Linux 子代理。

                        如果用户的问题不涉及以上子代理能力，你可以直接使用自己的知识进行回答。
                        在调用子代理之前，如果用户描述不明确，请先向用户确认具体需求再执行。
                        所有操作应返回清晰、简洁的结果说明。
                        """,
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
