using CC.Agents.ExcelAgents;
using CC.Agents.LinuxAgents;
using CC.Agents.PowerShellAgents;
using CC.Agents.SystemInfoAgents;
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
            var results = new List<TextSearchProvider.TextSearchResult>();

            //var store = new SqliteVectorStore("Data Source=vector.db");
            //var chunkCollection = store.GetCollection<int, DocumentChunk>("skhotels");

            //var queryVector = default(ReadOnlyMemory<float>);
            //try
            //{
            //    var queryEmbedder = new OllamaEmbeddingGenerator(
            //        new Uri("http://localhost:11434"),
            //        "qwen3-embedding"
            //    );
            //    var queryEmbedding = await queryEmbedder.GenerateAsync(query);
            //    queryVector = queryEmbedding.Vector;
            //}
            //catch (Exception)
            //{
            //    // 如果无法访问 Ollama 模型，直接返回空结果
            //    return results;
            //}

            //await using var searchEnumerator = chunkCollection.SearchAsync(queryVector, 3).GetAsyncEnumerator(cancellationToken);
            //while (await searchEnumerator.MoveNextAsync())
            //{
            //    var hit = searchEnumerator.Current;
            //    if (hit.Score > 0.3)
            //    {
            //        continue;
            //    }
            //    results.Add(new TextSearchProvider.TextSearchResult
            //    {
            //        Text = hit.Record?.Content ?? string.Empty
            //    });
            //}

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
                        你是一个智能助手协调器（Supervisor Agent）。

                        你的职责不是直接执行任务，而是根据用户需求选择合适的专业子代理。

                        你可以调用以下子代理：

                        1. ExcelAgent
                        负责所有 Excel 相关任务：
                        - 创建和打开 Excel 文件
                        - 读取和修改单元格
                        - 工作表管理
                        - 单元格格式设置
                        - 表格数据处理


                        2. PowerShellAgent
                        负责 Windows 系统操作：
                        - 执行 PowerShell 命令
                        - 管理 Windows 文件
                        - 查询系统状态
                        - 执行 Windows 自动化任务


                        3. LinuxAgent
                        负责 Linux 系统操作：
                        - 执行 Bash 命令
                        - 管理 Linux 文件
                        - 查询 Linux 系统状态
                        - 执行 Linux 自动化任务

                        4. SystemInfoAgent
                        判断当前系统环境。


                        任务分配规则：

                        - 用户请求 Excel 或表格操作时，调用 ExcelAgent。
                        - 用户请求 Windows 命令或 PowerShell 操作时，调用 PowerShellAgent。
                        - 用户请求 Linux 命令或 Linux 环境操作时，调用 LinuxAgent。


                        如果一个请求涉及多个领域，可以依次调用多个子代理，并综合它们的结果。


                        如果无法判断用户需求属于哪个代理：
                        先向用户询问澄清问题。


                        不要自己执行 Excel、PowerShell 或 Linux 操作。
                        这些任务必须委托给对应子代理。


                        对于普通知识问答，可以直接回答用户。


                        回答用户时：
                        - 简洁
                        - 明确
                        - 说明执行结果
                        """,
                    Tools =
                    [
                        ExcelAgent.CreateExcelAgent().AsAIFunction(),
                        LinuxAgent.CreateLinuxAgent().AsAIFunction(),
                        PowerShellAgent.CreatePowerShellAgent().AsAIFunction(),
                        SystemInfoAgent.CreateSystemInfoAgent().AsAIFunction()
                    ]
                },
                AIContextProviders = [new TextSearchProvider(SearchAdapter, textSearchOptions)]
            });

            return agent;
    }
}
