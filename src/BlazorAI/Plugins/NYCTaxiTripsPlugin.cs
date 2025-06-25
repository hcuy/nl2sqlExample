using Microsoft.SemanticKernel;
using System.ComponentModel;
using Microsoft.Data.SqlClient;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAI.Plugins
{
	public class NYCTaxiTripsPlugin
	{
		private readonly string _connectionString;

		public NYCTaxiTripsPlugin(IConfiguration configuration)
		{
			// Store your TSQL endpoint connection string in appsettings.json or user secrets
			_connectionString = configuration["FABRIC_TSQL_CONNECTION"]
				?? throw new ArgumentNullException("FABRIC_TSQL_CONNECTION");
		}

		[KernelFunction("query_nyc_taxi_trips")]
		[Description("Executes a TSQL query against the Fabric Data Lake to get information about NYC taxi trips and returns the results as a CSV string.")]
		public async Task<string> QueryNYCTaxiTrips([Description("The TSQL query to execute.")] string tsqlQuery)
		{
			try
			{
				using SqlConnection connection = new(_connectionString);
				await connection.OpenAsync();
				/*
				tsqlQuery = @"SELECT
                [puYear] AS Year,
                [puMonth] AS Month,
                AVG([tripDistance]) AS AverageTripDistance
				FROM [DemoLH].[dbo].[nyctaxi_raw]
				GROUP BY [puYear], [puMonth]
				ORDER BY [puYear], [puMonth];";
				*/
				using SqlCommand command = new(tsqlQuery, connection);
				using SqlDataReader reader = await command.ExecuteReaderAsync();

				StringBuilder csv = new();

				// Write headers
				for (int i = 0; i < reader.FieldCount; i++)
				{
					csv.Append(reader.GetName(i));
					if (i < reader.FieldCount - 1) csv.Append(",");
				}
				csv.AppendLine();

				// Write rows
				while (await reader.ReadAsync())
				{
					for (int i = 0; i < reader.FieldCount; i++)
					{
						csv.Append(reader[i]?.ToString());
						if (i < reader.FieldCount - 1) csv.Append(",");
					}
					csv.AppendLine();
				}

				return csv.ToString();
			}
			catch (SqlException ex)
			{
				return $"SQL Error: {ex.Message}";
			}
			catch (Exception ex)
			{
				return $"Error: {ex.Message}";
			}
		}
	}
}
