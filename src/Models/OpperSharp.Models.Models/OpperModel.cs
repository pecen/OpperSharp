using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace OpperSharp.Models.Models
{
	/// <summary>
	/// Represents a language model.
	/// </summary>
	public class OpperModel
	{
		[JsonProperty("id")]
		public string Id { get; set; } = string.Empty;

		[JsonProperty("name")]
		public string Name { get; set; } = string.Empty;

		[JsonProperty("provider")]
		public string? Provider { get; set; }

		[JsonProperty("capabilities")]
		public List<string>? Capabilities { get; set; }

		[JsonProperty("created_at")]
		public DateTime? CreatedAt { get; set; }
	}

	/// <summary>
	/// Represents a custom registered model.
	/// </summary>
	public class OpperCustomModel
	{
		[JsonProperty("id")]
		public string Id { get; set; } = string.Empty;

		[JsonProperty("name")]
		public string Name { get; set; } = string.Empty;

		[JsonProperty("identifier")]
		public string Identifier { get; set; } = string.Empty;

		[JsonProperty("extra")]
		public Dictionary<string, object>? Extra { get; set; }

		[JsonProperty("created_at")]
		public DateTime? CreatedAt { get; set; }

		[JsonProperty("updated_at")]
		public DateTime? UpdatedAt { get; set; }
	}
}
