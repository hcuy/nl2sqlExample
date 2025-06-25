using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.ChatCompletion;

namespace BlazorAI.Plugins
{
	public class NYCTaxiTSQLGeneratorPlugin
	{
		private readonly IChatCompletionService _chatCompletionService;

		public NYCTaxiTSQLGeneratorPlugin(IChatCompletionService chatCompletionService)
		{
			_chatCompletionService = chatCompletionService;
		}

		[KernelFunction("generate_nyc_taxi_sql")]
		[Description("Generates a TSQL query for the nyctaxi_raw table based on a natural language request.")]
		public async Task<string> GenerateNYCTaxiTSQLAsync(
			[Description("A natural language request about NYC taxi trips, e.g. 'average trip distance by month in 2022'")] string userRequest)
		{
			string prompt = $@"
You are a helpful assistant that writes safe TSQL queries for the following schema:

TABLE [dbo].[nyctaxi_raw](
    [vendorID] [varchar](8000) NULL,
    [tpepPickupDateTime] [datetime2](6) NULL,
    [tpepDropoffDateTime] [datetime2](6) NULL,
    [passengerCount] [int] NULL,
    [tripDistance] [float] NULL,
    [puLocationId] [varchar](8000) NULL,
    [doLocationId] [varchar](8000) NULL,
    [startLon] [float] NULL,
    [startLat] [float] NULL,
    [endLon] [float] NULL,
    [endLat] [float] NULL,
    [rateCodeId] [int] NULL,
    [storeAndFwdFlag] [varchar](8000) NULL,
    [paymentType] [varchar](8000) NULL,
    [fareAmount] [float] NULL,
    [extra] [float] NULL,
    [mtaTax] [float] NULL,
    [improvementSurcharge] [varchar](8000) NULL,
    [tipAmount] [float] NULL,
    [tollsAmount] [float] NULL,
    [totalAmount] [float] NULL,
    [puYear] [int] NULL,
    [puMonth] [int] NULL
)

Write a TSQL query for this request: '{userRequest}'
Only return the TSQL statement, nothing else.
";
			var tsqlQuery = await _chatCompletionService.GetChatMessageContentAsync(prompt);
			return tsqlQuery.ToString().Trim();
		}
	}
}
