using Newtonsoft.Json;
using System;

namespace OpperSharp.Models.Indexes
{
	/// <summary>
	/// A document that has been indexed.
	/// </summary>
	public class OpperIndexedDocument : OpperDocument
	{
		[JsonProperty("index_id")]
		public string? IndexId { get; set; }

		[JsonProperty("created_at")]
		public DateTime? CreatedAt { get; set; }

		[JsonProperty("updated_at")]
		public DateTime? UpdatedAt { get; set; }
	}
}
