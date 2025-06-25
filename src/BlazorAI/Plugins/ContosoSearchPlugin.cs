using Azure.Search.Documents;
using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using System.ComponentModel;
using System.Text.Json.Serialization;


#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

//https://learn.microsoft.com/en-us/semantic-kernel/concepts/plugins/using-data-retrieval-functions-for-rag
//https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/out-of-the-box-connectors/azure-ai-search-connector?pivots=programming-language-csharp

//Sample Data to use
//https://github.com/Azure-Samples/azure-search-sample-data

//oai Studio to upload/chunk documents
//https://devblogs.microsoft.com/semantic-kernel/azure-openai-on-your-data-with-semantic-kernel/



namespace BlazorAI.Plugins;

public class ContosoSearchPlugin
{
	private ITextEmbeddingGenerationService _textEmbeddingGenerationService;
	private SearchIndexClient _indexClient;

	public ContosoSearchPlugin(ITextEmbeddingGenerationService textEmbeddingGenerationService, SearchIndexClient indexClient)
	{
		_textEmbeddingGenerationService = textEmbeddingGenerationService;
		_indexClient = indexClient;
	}

	[KernelFunction("contoso_search")]
	[Description("Search documents for employer Contoso")]
	public async Task<string> SearchAsync([Description("The users optimized semantic search query")] string query)
	{
		// Convert string query to vector
		ReadOnlyMemory<float> embedding = await _textEmbeddingGenerationService.GenerateEmbeddingAsync(query);

		// Set the index to use in AI Search
		SearchClient searchClient = _indexClient.GetSearchClient("testing");

		// Configure request parameters
		VectorizedQuery vectorQuery = new(embedding);
		vectorQuery.Fields.Add("contentVector"); // name of the vector field from index schema

		SearchOptions searchOptions = new() { VectorSearch = new() { Queries = { vectorQuery } } };

		//var response = await searchClient.SearchAsync<SearchDocument>(searchOptions);

		// Perform search request
		Response<SearchResults<IndexSchema>> response = await searchClient.SearchAsync<IndexSchema>(searchOptions);

		//// Collect search results
		await foreach (SearchResult<IndexSchema> result in response.Value.GetResultsAsync())
		{
			return result.Document.Content; // Return text from first result
		}

		return string.Empty;
	}

	//This schema comes from the index schema in Azure AI Search
	private sealed class IndexSchema
	{
		[JsonPropertyName("content")]
		public string Content { get; set; }

		[JsonPropertyName("title")]
		public string Title { get; set; }

		[JsonPropertyName("url")]
		public string Url { get; set; }
	}
}
