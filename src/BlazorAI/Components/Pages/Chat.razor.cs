using Microsoft.AspNetCore.Components;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using BlazorAI.Plugins;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using Azure.Search.Documents.Indexes;
using Azure;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.TextToImage;


#pragma warning disable SKEXP0040 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace BlazorAI.Components.Pages;

public partial class Chat
{
	private ChatHistory? chatHistory;
	private Kernel? kernel;
	private OpenAIPromptExecutionSettings openAIPromptExecutionSettings;

	[Inject]
	public required IConfiguration Configuration { get; set; }
	[Inject]
	private ILoggerFactory LoggerFactory { get; set; } = null!;

	protected async Task InitializeSemanticKernel()
	{
		chatHistory = [];

		// Challenge 02 - Configure Semantic Kernel
		var kernelBuilder = Kernel.CreateBuilder();

		// Challenge 02 - Add OpenAI Chat Completion
		kernelBuilder.AddAzureOpenAIChatCompletion(
			Configuration["AOI_DEPLOYMODEL"]!, // Name of deployment, e.g. "gpt-4o".
			Configuration["AOI_ENDPOINT"]!,
			Configuration["AOI_API_KEY"]!);

		// Add Logger for Kernel
		kernelBuilder.Services.AddSingleton(LoggerFactory);

		// Challenge 03 and 04 - Services Required
		kernelBuilder.Services.AddHttpClient();

		// Challenge 05 - Register Azure AI Foundry Text Embeddings Generation
		kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(
			deploymentName: Configuration["EMBEDDINGS_DEPLOYMODEL"]!, // Name of deployment, e.g. "text-embedding-ada-002".
			endpoint: Configuration["AOI_ENDPOINT"]!,           // Name of Azure OpenAI service endpoint, e.g. https://myaiservice.openai.azure.com.
			apiKey: Configuration["AOI_API_KEY"]!
		);

		// Challenge 05 - Register Search Index
		kernelBuilder.Services.AddSingleton<SearchIndexClient>(
			sp => new SearchIndexClient(
				new Uri(Configuration["AI_SEARCH_URL"]!),
				new AzureKeyCredential(Configuration["AI_SEARCH_KEY"]!)));
		kernelBuilder.AddAzureAISearchVectorStore();

		// Challenge 07 - Add Azure AI Foundry Text To Image
		kernelBuilder.AddAzureOpenAITextToImage(
			Configuration["DALLE_DEPLOYMODEL"]!,
			Configuration["AOI_ENDPOINT"]!,
			Configuration["AOI_API_KEY"]!);

		// Challenge 02 - Finalize Kernel Builder
		kernel = kernelBuilder.Build();

		// Challenge 03, 04, 05, & 07 - Add Plugins
		await AddPlugins();

		// Challenge 03 - Create OpenAIPromptExecutionSettings
		openAIPromptExecutionSettings = new()
		{
			FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(), 
			ChatSystemPrompt = "You are an AI assistant that helps people find information.  Ask follow-up questions if something is unclear or you need more information to complete a task.",
			Temperature = 0.9f
		};

	}


	private async Task AddPlugins()
	{
		/*
		// Challenge 03 - Add Time Plugin
		kernel.Plugins.AddFromType<MyTimePlugin>("TimePlugin");

		kernel.Plugins.AddFromObject(new GeocodingPlugin(kernel.Services.GetRequiredService<IHttpClientFactory>(), Configuration), "GeocodingPlugin");

		kernel.Plugins.AddFromObject(new WeatherPlugin(kernel.Services.GetRequiredService<IHttpClientFactory>()), "WeatherPlugin");

		// Challenge 04 - Import OpenAPI Spec
		await kernel.ImportPluginFromOpenApiAsync(
		   pluginName: "WorkItemsPlugin",
		   uri: new Uri("http://localhost:5115/swagger/v1/swagger.json"),
		   executionParameters: new OpenApiFunctionExecutionParameters()
		   {
			   // Determines whether payload parameter names are augmented with namespaces.
			   // Namespaces prevent naming conflicts by adding the parent parameter name
			   // as a prefix, separated by dots
			   EnablePayloadNamespacing = true,
			   HttpClient = kernel.Services.GetRequiredService<IHttpClientFactory>().CreateClient("GetWorkItemsClient")
		   }
		);
		
		// Challenge 05 - Add Search Plugin
		kernel.Plugins.AddFromType<ContosoSearchPlugin2>("ContosoSearchPlugin", kernel.Services);

		// Challenge 07 - Text To Image Plugin
		var plugin = new TextToImagePlugin(kernel.GetRequiredService<ITextToImageService>());
		kernel.Plugins.AddFromObject(plugin, "TextToImagePlugin");
		*/

		kernel.Plugins.AddFromType<NYCTaxiTSQLGeneratorPlugin>("NYCTaxiTSQLGeneratorPlugin", kernel.Services);
		kernel.Plugins.AddFromObject(new NYCTaxiTripsPlugin(Configuration), "NYCTaxiTripsPlugin");

	}


	private async Task SendMessage()
	{
		if (!string.IsNullOrWhiteSpace(newMessage) && chatHistory != null)
		{
			// This tells Blazor the UI is going to be updated.
			StateHasChanged();
			loading = true;
			// Copy the user message to a local variable and clear the newMessage field in the UI
			var userMessage = newMessage;
			newMessage = string.Empty;
			StateHasChanged();

			// Challenge 02 - Retrieve the chat completion service
			var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

			// Challenge 02 - Update Chat History
			chatHistory.AddUserMessage(userMessage);

			// Challenge 02 - Send a message to the chat completion service
			var response = await chatCompletionService.GetChatMessageContentAsync(
				chatHistory,
				kernel: kernel, 
				executionSettings: openAIPromptExecutionSettings
			);

			// Challenge 02 - Add Response to the Chat History object
			chatHistory.Add(response);

			loading = false;
		}
	}

}
