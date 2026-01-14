using Newtonsoft.Json;

namespace OpperSharp.Models.Chat
{
	/// <summary>
	/// Token usage information.
	/// </summary>
	public class OpperUsage
	{
		[JsonProperty("prompt_tokens")]
		public int PromptTokens { get; set; }

		[JsonProperty("completion_tokens")]
		public int CompletionTokens { get; set; }

		[JsonProperty("total_tokens")]
		public int TotalTokens { get; set; }
	}
}
