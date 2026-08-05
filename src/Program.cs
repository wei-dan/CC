using CommunityToolkit.VectorData.SqliteVec;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.ComponentModel;

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
            Tools = 
            [
                AIFunctionFactory.Create(ExcelAgent.OpenExcelWorkbook),
                AIFunctionFactory.Create(ExcelAgent.ReadExcelCell),
                AIFunctionFactory.Create(ExcelAgent.WriteExcelCell),
                AIFunctionFactory.Create(ExcelAgent.ReadExcelRange),
                AIFunctionFactory.Create(ExcelAgent.GetExcelUsedRange),
                AIFunctionFactory.Create(ExcelAgent.AppendExcelRow),
                AIFunctionFactory.Create(ExcelAgent.CreateExcelWorkbook),
                AIFunctionFactory.Create(ExcelAgent.AddExcelSheet),
                AIFunctionFactory.Create(ExcelAgent.DeleteExcelSheet),
                AIFunctionFactory.Create(ExcelAgent.CheckExcelFileExists),
                AIFunctionFactory.Create(ExcelAgent.SetCellBackgroundColor),
                AIFunctionFactory.Create(ExcelAgent.SetCellFontColor),
                AIFunctionFactory.Create(ExcelAgent.SetCellFontBold),
                AIFunctionFactory.Create(ExcelAgent.SetCellFontItalic),
                AIFunctionFactory.Create(ExcelAgent.SetCellFontSize),
                AIFunctionFactory.Create(ExcelAgent.SetCellBorder),
                AIFunctionFactory.Create(ExcelAgent.SetColumnWidth),
                AIFunctionFactory.Create(ExcelAgent.SetRowHeight),
                AIFunctionFactory.Create(ExcelAgent.MergeCells),
                AIFunctionFactory.Create(ExcelAgent.UnmergeCells),
                AIFunctionFactory.Create(ExcelAgent.AutoFitColumns),
                AIFunctionFactory.Create(ExcelAgent.AutoFitRows),
                AIFunctionFactory.Create(PowerShellAgent.RunPowerShell),
                AIFunctionFactory.Create(LinuxAgent.RunLinuxCommand),
                AIFunctionFactory.Create(VectorDatabaseAgent.AddTextChunk),
                AIFunctionFactory.Create(VectorDatabaseAgent.AddFileChunks)
            ]
        },
        AIContextProviders = [new TextSearchProvider(SearchAdapter, textSearchOptions)]
    });

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
    await foreach (var update in agent.RunStreamingAsync(input, session))
    {
        if (!string.IsNullOrEmpty(update?.Text))
        {
            Console.Write(update.Text);
        }
    }
    Console.ResetColor();
    Console.WriteLine();
}
