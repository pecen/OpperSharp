using Newtonsoft.Json;
using System.Collections.Generic;

namespace OpperSharp.Models.Embeddings
{
	/// <summary>
	/// Response from embedding creation.
	/// </summary>
	public class OpperEmbeddingResponse
	{
		[JsonProperty("data")]
		public List<OpperEmbeddingData> Data { get; set; } = new();

		[JsonProperty("model")]
		public string? Model { get; set; }

		[JsonProperty("usage")]
		public OpperEmbeddingUsage? Usage { get; set; }
	}

	/// <summary>
	/// Single embedding data.
	/// </summary>
	public class OpperEmbeddingData
	{
		[JsonProperty("embedding")]
		public List<double> Embedding { get; set; } = new();

		[JsonProperty("index")]
		public int Index { get; set; }
	}

	/// <summary>
	/// Usage information for embeddings.
	/// </summary>
	public class OpperEmbeddingUsage
	{
		[JsonProperty("prompt_tokens")]
		public int PromptTokens { get; set; }

		[JsonProperty("total_tokens")]
		public int TotalTokens { get; set; }
	}
}
