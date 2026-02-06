using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace OpperSharp.Models.Models
{
	/// <summary>
	/// Represents a model alias with fallback support.
	/// </summary>
	public class OpperModelAlias
	{
		[JsonProperty("id")]
		public string Id { get; set; } = string.Empty;

		[JsonProperty("name")]
		public string Name { get; set; } = string.Empty;

		[JsonProperty("fallback_models")]
		public List<string> FallbackModels { get; set; } = new();

		[JsonProperty("description")]
		public string? Description { get; set; }

		[JsonProperty("created_at")]
		public DateTime? CreatedAt { get; set; }

		[JsonProperty("updated_at")]
		public DateTime? UpdatedAt { get; set; }
	}
}
