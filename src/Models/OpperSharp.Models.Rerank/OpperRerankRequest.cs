using Newtonsoft.Json;
using System.Collections.Generic;

namespace OpperSharp.Models.Rerank
{
	public class OpperRerankRequest
	{
		[JsonProperty("query")]
		public string Query { get; set; } = string.Empty;

		[JsonProperty("documents")]
		public List<object> Documents { get; set; } = new();

		[JsonProperty("model")]
		public string Model { get; set; } = string.Empty;

		[JsonProperty("top_k")]
		public int? TopK { get; set; }

		[JsonProperty("return_documents")]
		public bool? ReturnDocuments { get; set; }
	}

	public class OpperRerankResponse
	{
		[JsonProperty("results")]
		public List<OpperRerankResult> Results { get; set; } = new();
	}

	public class OpperRerankResult
	{
		[JsonProperty("index")]
		public int Index { get; set; }

		[JsonProperty("relevance_score")]
		public double RelevanceScore { get; set; }

		[JsonProperty("document")]
		public object? Document { get; set; }
	}
}
