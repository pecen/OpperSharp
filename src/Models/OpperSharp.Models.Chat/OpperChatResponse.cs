using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace OpperSharp.Models.Chat
{
	/// <summary>
	/// Response from a chat completion request.
	/// </summary>
	public class OpperChatResponse
	{
		[JsonProperty("id")]
		public string? Id { get; set; }

		[JsonProperty("choices")]
		public List<OpperChatChoice> Choices { get; set; } = new();

		[JsonProperty("usage")]
		public OpperUsage? Usage { get; set; }

		[JsonProperty("model")]
		public string? Model { get; set; }

		[JsonProperty("created")]
		public long? Created { get; set; }

		/// <summary>
		/// Gets the first choice's message content.
		/// </summary>
		public string? Content => Choices.FirstOrDefault()?.Message?.Content;
	}
}
