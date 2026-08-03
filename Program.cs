using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.ComponentModel;

const string apiKey = "sk-c7366fcac5aa4023827e049e7a714705"; // "sk-ws-H.EIIMERL.YOZt.MEUCIEnJYx_DqGa8aGadlD1AzkUXKik4SYqkIaYjstnlNHpcAiEA40Q3ru1LsKM_OQ_HO32SbYCnl-M7lWgJhTWkIk3K5l0";

TextSearchProviderOptions textSearchOptions = new()
{
    SearchTime = TextSearchProviderOptions.TextSearchBehavior.BeforeAIInvoke,
};
static Task<IEnumerable<TextSearchProvider.TextSearchResult>> SearchAdapter(string query, CancellationToken cancellationToken)
{
    List<TextSearchProvider.TextSearchResult> results = new();
    //results.Add(new()
    //{
    //    Text = "I Like Play basketball"
    //});
    return Task.FromResult<IEnumerable<TextSearchProvider.TextSearchResult>>(results);
}

AIAgent agent = new OpenAIClient(
        new ApiKeyCredential(apiKey),
        new OpenAIClientOptions
        {
            Endpoint = new Uri("https://api.deepseek.com") //("https://dashscope.aliyuncs.com/compatible-mode/v1")
        }
    )
    .GetChatClient("deepseek-v4-pro") //("qwen3.7-plus")
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
                AIFunctionFactory.Create(ExcelAgent.AutoFitRows)
                //AIFunctionFactory.Create(RunPowerShell)
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

    var response = await agent.RunAsync(input, session);

    Console.WriteLine($"Bot: { response.Text }");
}
