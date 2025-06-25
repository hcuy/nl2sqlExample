using System.ComponentModel;
using System.Text.Json.Serialization;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;

public class ContosoSearchPlugin2
{
	private readonly ITextEmbeddingGenerationService _textEmbeddingGenerationService;
	private readonly SearchIndexClient _indexClient;

	public ContosoSearchPlugin2(ITextEmbeddingGenerationService textEmbeddingGenerationService, SearchIndexClient indexClient)
	{
		_textEmbeddingGenerationService = textEmbeddingGenerationService;
		_indexClient = indexClient;
	}

	[KernelFunction("search_contoso_docs")]
	[Description("Search the Contoso Employee Handbook for a given query.")]
	public async Task<string> SearchAsync(string query)
	{
		// Convert string query to vector
		ReadOnlyMemory<float> embedding = await _textEmbeddingGenerationService.GenerateEmbeddingAsync(query);

		// Get client for search operations
		SearchClient searchClient = _indexClient.GetSearchClient("employeehandbook");

		// Configure request parameters
		VectorizedQuery vectorQuery = new(embedding);
		vectorQuery.Fields.Add("contentVector");

		SearchOptions searchOptions = new() { VectorSearch = new() { Queries = { vectorQuery } } };

		// Perform search request
		Response<SearchResults<IndexSchema>> response = await searchClient.SearchAsync<IndexSchema>(searchOptions);

		// Collect search results
		await foreach (SearchResult<IndexSchema> result in response.Value.GetResultsAsync())
		{
			return result.Document.Content; // Return text from first result
		}

		return string.Empty;
	}

	private sealed class IndexSchema
	{
		[JsonPropertyName("content")]
		public string Content { get; set; }

		[JsonPropertyName("contentVector")]
		public ReadOnlyMemory<float> ContentVector { get; set; }
	}
}
