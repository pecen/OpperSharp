using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace OpperSharp.Models.Chat
{
	/// <summary>
	/// Streaming chat response chunk.
	/// </summary>
	public class OpperChatStreamChunk
	{
		[JsonProperty("id")]
		public string? Id { get; set; }

		[JsonProperty("choices")]
		public List<OpperChatStreamChoice> Choices { get; set; } = new();

		/// <summary>
		/// Gets the delta content from the first choice.
		/// </summary>
		public string? DeltaContent => Choices.FirstOrDefault()?.Delta?.Content;
	}
}
