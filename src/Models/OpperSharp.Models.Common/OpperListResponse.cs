using Newtonsoft.Json;
using System.Collections.Generic;

namespace OpperSharp.Models.Common
{
	/// <summary>
	/// Generic list response wrapper for paginated API responses.
	/// </summary>
	public class OpperListResponse<T>
	{
		[JsonProperty("data")]
		public List<T> Data { get; set; } = new();

		[JsonProperty("total")]
		public int Total { get; set; }

		[JsonProperty("has_more")]
		public bool HasMore { get; set; }

		[JsonProperty("cursor")]
		public string? Cursor { get; set; }
	}
}
