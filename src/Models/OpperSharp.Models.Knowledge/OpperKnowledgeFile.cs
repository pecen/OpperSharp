using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace OpperSharp.Models.Knowledge
{
	/// <summary>
	/// Represents a file in a knowledge base.
	/// </summary>
	public class OpperKnowledgeFile
	{
		[JsonProperty("id")]
		public string Id { get; set; } = string.Empty;

		[JsonProperty("original_filename")]
		public string Filename { get; set; } = string.Empty;

		[JsonProperty("content_type")]
		public string? ContentType { get; set; }

		[JsonProperty("size")]
		public long? Size { get; set; }

		[JsonProperty("status")]
		public string? Status { get; set; }

		[JsonProperty("document_id")]
		public long? DocumentId { get; set; }

		[JsonProperty("metadata")]
		public Dictionary<string, object>? Metadata { get; set; }

		[JsonProperty("configuration")]
		public Dictionary<string, object>? Configuration { get; set; }

		[JsonProperty("created_at")]
		public DateTime? CreatedAt { get; set; }

		[JsonProperty("updated_at")]
		public DateTime? UpdatedAt { get; set; }
	}
}
