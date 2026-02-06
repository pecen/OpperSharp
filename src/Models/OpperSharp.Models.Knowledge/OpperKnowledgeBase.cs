using Newtonsoft.Json;
using System;

namespace OpperSharp.Models.Knowledge
{
	/// <summary>
	/// Represents an Opper AI knowledge base for file-based RAG.
	/// </summary>
	public class OpperKnowledgeBase
	{
		[JsonProperty("id")]
		public string Id { get; set; } = string.Empty;

		[JsonProperty("name")]
		public string Name { get; set; } = string.Empty;

		[JsonProperty("embedding_model")]
		public string? EmbeddingModel { get; set; }

		[JsonProperty("created_at")]
		public DateTime? CreatedAt { get; set; }

		[JsonProperty("updated_at")]
		public DateTime? UpdatedAt { get; set; }
	}
}
