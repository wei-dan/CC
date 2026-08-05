using CommunityToolkit.VectorData.SqliteVec;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace CC.Agents.ExcelAgents;

public static class ExcelAgent
{
    public static AIAgent CreateExcelAgent()
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
                Name = "ExcelAgent",
                ChatOptions = new()
                {
                    Tools =
                    [
                        AIFunctionFactory.Create(ExcelWorkbookAgentFunctions.OpenExcelWorkbook),
                        AIFunctionFactory.Create(ExcelWorkbookAgentFunctions.CreateExcelWorkbook),
                        AIFunctionFactory.Create(ExcelWorkbookAgentFunctions.AddExcelSheet),
                        AIFunctionFactory.Create(ExcelWorkbookAgentFunctions.DeleteExcelSheet),
                        AIFunctionFactory.Create(ExcelWorkbookAgentFunctions.CheckExcelFileExists),
                        AIFunctionFactory.Create(ExcelWorkbookAgentFunctions.GetExcelUsedRange),

                        AIFunctionFactory.Create(ExcelDataAgentFunctions.ReadExcelCell),
                        AIFunctionFactory.Create(ExcelDataAgentFunctions.WriteExcelCell),
                        AIFunctionFactory.Create(ExcelDataAgentFunctions.ReadExcelRange),
                        AIFunctionFactory.Create(ExcelDataAgentFunctions.AppendExcelRow),
                        
                        AIFunctionFactory.Create(ExcelStyleAgentFunctions.SetCellBackgroundColor),
                        AIFunctionFactory.Create(ExcelStyleAgentFunctions.SetCellFontColor),
                        AIFunctionFactory.Create(ExcelStyleAgentFunctions.SetCellFontBold),
                        AIFunctionFactory.Create(ExcelStyleAgentFunctions.SetCellFontItalic),
                        AIFunctionFactory.Create(ExcelStyleAgentFunctions.SetCellFontSize),
                        AIFunctionFactory.Create(ExcelStyleAgentFunctions.SetCellBorder),
                        
                        AIFunctionFactory.Create(ExcelLayoutAgentFunctions.SetColumnWidth),
                        AIFunctionFactory.Create(ExcelLayoutAgentFunctions.SetRowHeight),
                        AIFunctionFactory.Create(ExcelLayoutAgentFunctions.MergeCells),
                        AIFunctionFactory.Create(ExcelLayoutAgentFunctions.UnmergeCells),
                        AIFunctionFactory.Create(ExcelLayoutAgentFunctions.AutoFitColumns),
                        AIFunctionFactory.Create(ExcelLayoutAgentFunctions.AutoFitRows)
                    ]
                },
                AIContextProviders = [new TextSearchProvider(SearchAdapter, textSearchOptions)]
            });
        return agent;
    }
}