using Newtonsoft.Json;

namespace OpperSharp.Models.Chat
{
	/// <summary>
	/// A single chat completion choice.
	/// </summary>
	public class OpperChatChoice
	{
		[JsonProperty("index")]
		public int Index { get; set; }

		[JsonProperty("message")]
		public OpperMessage? Message { get; set; }

		[JsonProperty("finish_reason")]
		public string? FinishReason { get; set; }
	}
}
