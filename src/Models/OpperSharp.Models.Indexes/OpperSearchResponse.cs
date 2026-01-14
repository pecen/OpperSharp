using Newtonsoft.Json;
using System.Collections.Generic;

namespace OpperSharp.Models.Indexes
{
	/// <summary>
	/// Response from an index query/search.
	/// </summary>
	public class OpperSearchResponse
	{
		[JsonProperty("results")]
		public List<OpperSearchResult> Results { get; set; } = new();
	}

	/// <summary>
	/// A single search result.
	/// </summary>
	public class OpperSearchResult
	{
		[JsonProperty("content")]
		public string Content { get; set; } = string.Empty;

		[JsonProperty("metadata")]
		public Dictionary<string, object>? Metadata { get; set; }

		[JsonProperty("score")]
		public double Score { get; set; }

		[JsonProperty("id")]
		public string? Id { get; set; }

		[JsonProperty("index_id")]
		public string? IndexId { get; set; }
	}
}
