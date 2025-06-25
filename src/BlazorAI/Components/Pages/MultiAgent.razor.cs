using Microsoft.AspNetCore.Components;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;


#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace BlazorAI.Components.Pages
{
    public partial class MultiAgent
    {
        private ChatHistory? chatHistory;
        private IChatCompletionService? chatCompletionService;
        private OpenAIPromptExecutionSettings? openAIPromptExecutionSettings;
        private Kernel? kernel;

        [Inject]
        public required IConfiguration Configuration { get; set; }

        [Inject]
        private ILoggerFactory LoggerFactory { get; set; } = null!;

        private List<ChatCompletionAgent> Agents { get; set; } = [];

        private AgentGroupChat AgentGroupChat;


        protected async Task InitializeSemanticKernel()
        {
            chatHistory = [];

            var kernelBuilder = Kernel.CreateBuilder();

            kernelBuilder.AddAzureOpenAIChatCompletion(
                Configuration["AOI_DEPLOYMODEL"] ?? "gpt-35-turbo",
                Configuration["AOI_ENDPOINT"]!,
                Configuration["AOI_API_KEY"]!);

            kernelBuilder.Services.AddSingleton(LoggerFactory);

            kernel = kernelBuilder.Build();

            await CreateAgents();

			// create Agent Group Chat [AgentGroupChat]
			AgentGroupChat = new AgentGroupChat(Agents.ToArray())
			{
				ExecutionSettings = new()
				{
					TerminationStrategy = new ApprovalTerminationStrategy()
					{
						Agents = [Agents.Last()],
						MaximumIterations = 6,
						AutomaticReset = true
					}
				}
			};

		}

        private async Task CreateAgents()
        {
			// Create a Business Analyst Agent [ChatCompletionAgent] and add it to the Agents List.
			// ChatCompletionAgent takes Instructions, a Name (No Spaces allowed), and the Kernel.
			string businessInstructions = "You are a Business Analyst who will take the requirements from the user (also known as a 'customer') " +
				"\r\nand create a project plan for creating the requested app. The Business Analyst understands the user " +
				"\r\nrequirements and creates detailed documents with requirements and costing. The documents should be " +
				"\r\nusable by the SoftwareEngineer as a reference for implementing the required features, and by the " +
				"\r\nProduct Owner for reference to determine if the application delivered by the Software Engineer meets " +
				"\r\nall of the user's requirements.";
			ChatCompletionAgent businessAnalystAgent = new()
			{
				Kernel = kernel,
				Name = "BusinessAnalyst",
				Instructions = businessInstructions
			};
			Agents.Add(businessAnalystAgent);

			// Create a Software Engineer Agent [ChatCompletionAgent] and add it to the Agents List.
			// ChatCompletionAgent takes Instructions, a Name (No Spaces allowed), and the Kernel.
			string softwareInstructions = "You are a Software Engineer, and your goal is to create a web app using HTML and JavaScript " +
				"\r\nby taking into consideration all the requirements given by the Business Analyst. The application should " +
				"\r\nimplement all the requested features. Deliver the code to the Product Owner for review when completed. " +
				"\r\nYou can also ask questions of the BusinessAnalyst to clarify any requirements that are unclear.";
			ChatCompletionAgent softwareEngineerAgent = new()
			{
				Kernel = kernel,
				Name = "SoftwareEngineer",
				Instructions = softwareInstructions
			};
			Agents.Add(softwareEngineerAgent);

			// Create a Product Owner Agent [ChatCompletionAgent] and add it to the Agents List.
			// ChatCompletionAgent takes Instructions, a Name (No Spaces allowed), and the Kernel.
			string productInstructions = "You are the Product Owner who will review the software engineer's code to ensure all user " +
				"\r\nrequirements are completed. You are the guardian of quality, ensuring the final product meets " +
				"\r\nall specifications and receives the green light for release. Once all client requirements are " +
				"\r\ncompleted, you can approve the request by just responding 'approve'. Do not ask any other agent " +
				"\r\nnor the user for approval. If there are missing features, you will need to send a request back " +
				"\r\nto the SoftwareEngineer or BusinessAnalyst with details of the defect. To approve, respond with " +
				"\r\nthe token %APPR%.";
			ChatCompletionAgent productOwnerAgent = new()
			{
				Kernel = kernel,
				Name = "ProductOwner",
				Instructions = productInstructions
			};
			Agents.Add(productOwnerAgent);

		}

		private async Task AddPlugins()
        {

        }

        private async Task SendMessage()
        {
            // Copy the message from the user input - just like in Chat.razor.cs
            var userMessage = MessageInput;
            MessageInput = string.Empty;
            loading = true;
            chatHistory.AddUserMessage(userMessage);
            StateHasChanged();

			// Add a new ChatMessageContent to the AgentGroupChat with the User role, and userMessage contents
			AgentGroupChat.AddChatMessage(new ChatMessageContent(AuthorRole.User, userMessage));
			StateHasChanged();

			try
            {
				// Use async foreach to iterate over the messages from the AgentGroupChat
				await foreach (var message in AgentGroupChat.InvokeAsync())
				{
					// Add each message to the chatHistory
					chatHistory.Add(message);
					StateHasChanged();
				}

			}
            catch (Exception e)
            {
                logger.LogError(e, "Error while trying to send message to agents.");
            }
            loading = false;
        }
    }

    sealed class ApprovalTerminationStrategy : TerminationStrategy
    {
        // Setup your ApprovalTerminationStrategy here
        // Use the history[^1].Content property to check if the message contains the token
        // you are looking for. Return true if the token is found, false otherwise.
        protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken = default)
            => Task.FromResult(history[^1].Content?.Contains("%APPR%", StringComparison.OrdinalIgnoreCase) ?? false);
	}
}
