using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace OpperSharp.Models.Functions
{
	/// <summary>
	/// Represents an Opper AI function.
	/// </summary>
	public class OpperFunction
	{
		[JsonProperty("id")]
		public string Id { get; set; } = string.Empty;

		[JsonProperty("path")]
		public string Path { get; set; } = string.Empty;

		[JsonProperty("name")]
		public string? Name { get; set; }

		[JsonProperty("description")]
		public string? Description { get; set; }

		[JsonProperty("instructions")]
		public string? Instructions { get; set; }

		[JsonProperty("input_schema")]
		public JObject? InputSchema { get; set; }

		[JsonProperty("output_schema")]
		public JObject? OutputSchema { get; set; }

		[JsonProperty("model")]
		public string? Model { get; set; }

		[JsonProperty("index_ids")]
		public List<string>? IndexIds { get; set; }

		[JsonProperty("created_at")]
		public DateTime? CreatedAt { get; set; }

		[JsonProperty("updated_at")]
		public DateTime? UpdatedAt { get; set; }
	}
}
