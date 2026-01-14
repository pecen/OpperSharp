using Newtonsoft.Json;

namespace OpperSharp.Models.Chat
{
	/// <summary>
	/// Streaming chat choice.
	/// </summary>
	public class OpperChatStreamChoice
	{
		[JsonProperty("index")]
		public int Index { get; set; }

		[JsonProperty("delta")]
		public OpperMessage? Delta { get; set; }

		[JsonProperty("finish_reason")]
		public string? FinishReason { get; set; }
	}
}
