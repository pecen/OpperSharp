using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace OpperSharp.Models.Functions
{
	/// <summary>
	/// Definition for creating or updating a function.
	/// </summary>
	public class OpperFunctionDefinition
	{
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

		[JsonProperty("temperature")]
		public double? Temperature { get; set; }

		[JsonProperty("index_ids")]
		public List<string>? IndexIds { get; set; }

		[JsonProperty("cache")]
		public bool? Cache { get; set; }

		[JsonProperty("examples")]
		public List<OpperFunctionExample>? Examples { get; set; }
	}

	/// <summary>
	/// Example input/output pair for few-shot learning.
	/// </summary>
	public class OpperFunctionExample
	{
		[JsonProperty("input")]
		public Dictionary<string, object>? Input { get; set; }

		[JsonProperty("output")]
		public Dictionary<string, object>? Output { get; set; }
	}
}
