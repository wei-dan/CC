using CommunityToolkit.VectorData.SqliteVec;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.ComponentModel;

const string apiKey = "sk-c7366fcac5aa4023827e049e7a714705";

var embeddingGenerator = new OllamaEmbeddingGenerator(
                    new Uri("http://localhost:11434"),
                    "qwen3-embedding"
                );

var vectorStore = new SqliteVectorStore("Data Source=vector.db");
var collection = vectorStore.GetCollection<int, Hotel>("skhotels");
await collection.EnsureCollectionExistsAsync();
await collection.UpsertAsync(new Hotel
{
    HotelId = 1,
    HotelName = "Hotel California",
    Description = "我老婆的名字叫郭敏，我女儿叫魏伊诺",
    DescriptionEmbedding = (await embeddingGenerator.GenerateAsync("我老婆的名字叫郭敏，我女儿叫魏伊诺")).Vector
});


TextSearchProviderOptions textSearchOptions = new()
{
    SearchTime = TextSearchProviderOptions.TextSearchBehavior.BeforeAIInvoke,
};
static async Task<IEnumerable<TextSearchProvider.TextSearchResult>> SearchAdapter(string query, CancellationToken cancellationToken)
{
    var vectorStore = new SqliteVectorStore("Data Source=vector.db");
    var collection = vectorStore.GetCollection<int, Hotel>("skhotels");
    List<TextSearchProvider.TextSearchResult> results = new();
    var embeddingGenerator = new OllamaEmbeddingGenerator(
                    new Uri("http://localhost:11434"),
                    "qwen3-embedding"
                );
    var sss = await embeddingGenerator.GenerateAsync(query);
    await using var enumerator = collection.SearchAsync(sss.Vector, 3).GetAsyncEnumerator(cancellationToken);
    while (await enumerator.MoveNextAsync())
    {
        var s = enumerator.Current;
        if (s.Score > 0.3)
        {
            continue;
        }
        results.Add(new TextSearchProvider.TextSearchResult
        {
            Text = s.Record?.Description ?? string.Empty
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
                AIFunctionFactory.Create(LinuxAgent.RunLinuxCommand)
            ]
        },
        AIContextProviders = [new TextSearchProvider(SearchAdapter, textSearchOptions)]
    });

AgentSession session = await agent.CreateSessionAsync();

while (true)
{
    Console.Write("You: ");
    string? input = Console.ReadLine();

    if (input is null)
    {
        continue;
    }

    Console.Write("Bot: ");
    await foreach (var update in agent.RunStreamingAsync(input, session))
    {
        if (!string.IsNullOrEmpty(update?.Text))
        {
            Console.Write(update.Text);
        }
    }
    Console.WriteLine();
}


public class Hotel
{
    [VectorStoreKey]
    public int HotelId { get; set; }

    [VectorStoreData(StorageName = "hotel_name")]
    public string? HotelName { get; set; }

    [VectorStoreData(StorageName = "hotel_description")]
    public string? Description { get; set; }

    [VectorStoreVector(dimensions: 4096, DistanceFunction = DistanceFunction.CosineDistance)]
    public ReadOnlyMemory<float>? DescriptionEmbedding { get; set; }
}
