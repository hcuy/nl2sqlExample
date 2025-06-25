using Microsoft.SemanticKernel.TextToImage;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;

namespace BlazorAI.Plugins
{
	public class TextToImagePlugin
	{
		private readonly ITextToImageService _textToImageService;

		public TextToImagePlugin(ITextToImageService textToImageService)
		{
			_textToImageService = textToImageService;
		}

		/// <summary>
		/// Challenge-07: Text to Image
		/// Returns an image link generated from the text description
		/// </summary>
		/// <param name="imageDescription"></param>
		/// <returns></returns>
		/// <example
		/// Example Prompts:
		/// Example 1: 
		///     1. Create a picture of a cute baby otter playing with a ball
		/// Example 2:
		///     1. generate a detailed childrens story about a dragon and a little girl that go on an adventure together
		///     2. generate a cartoon style image suitable for children of a major scene in the story
		/// 
		/// </example>
		[KernelFunction("generate_image_from_text")]
		[Description("returns an image url from a text description")]
		[return: Description("URL of the generated image")]
		public async Task<string> GetImageURLAsync([Description("Descriptive prompt optimized for DALL-E")] string imageDescription)
		{
			var generatedImages = await _textToImageService.GetImageContentsAsync(
				new TextContent(imageDescription),
				new OpenAITextToImageExecutionSettings { Size = (Width: 1024, Height: 1024) });

			return generatedImages[0].Uri?.ToString() ?? string.Empty;
		}
	}
}
