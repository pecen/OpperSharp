using Newtonsoft.Json;
using System;

namespace OpperSharp.Models.Indexes
{
	/// <summary>
	/// Represents an Opper AI index for vector storage and retrieval.
	/// </summary>
	public class OpperIndex
	{
		[JsonProperty("id")]
		public string Id { get; set; } = string.Empty;

		[JsonProperty("name")]
		public string Name { get; set; } = string.Empty;

		[JsonProperty("description")]
		public string? Description { get; set; }

		[JsonProperty("document_count")]
		public int DocumentCount { get; set; }

		[JsonProperty("created_at")]
		public DateTime? CreatedAt { get; set; }

		[JsonProperty("updated_at")]
		public DateTime? UpdatedAt { get; set; }
	}
}
