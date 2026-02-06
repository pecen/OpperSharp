using Newtonsoft.Json;
using System;

namespace OpperSharp.Models.Datasets
{
	/// <summary>
	/// Represents a dataset entry for training/evaluation.
	/// </summary>
	public class OpperDatasetEntry
	{
		[JsonProperty("id")]
		public string Id { get; set; } = string.Empty;

		[JsonProperty("input")]
		public object Input { get; set; } = new();

		[JsonProperty("output")]
		public object Output { get; set; } = new();

		[JsonProperty("expected")]
		public object? Expected { get; set; }

		[JsonProperty("comment")]
		public string? Comment { get; set; }

		[JsonProperty("created_at")]
		public DateTime? CreatedAt { get; set; }

		[JsonProperty("updated_at")]
		public DateTime? UpdatedAt { get; set; }
	}
}
