using Newtonsoft.Json;
using System.Collections.Generic;

namespace OpperSharp.Models.Indexes
{
	/// <summary>
	/// Document to be added to an index.
	/// </summary>
	public class OpperDocument
	{
		[JsonProperty("content")]
		public string Content { get; set; } = string.Empty;

		[JsonProperty("metadata")]
		public Dictionary<string, object>? Metadata { get; set; }

		[JsonProperty("id")]
		public string? Id { get; set; }
	}
}
