using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace OpperSharp.Models.Analytics
{
	public class OpperAnalyticsResponse
	{
		[JsonProperty("data")]
		public List<OpperAnalyticsData> Data { get; set; } = new();
	}

	public class OpperAnalyticsData
	{
		[JsonProperty("timestamp")]
		public DateTime? Timestamp { get; set; }

		[JsonProperty("metrics")]
		public Dictionary<string, object>? Metrics { get; set; }
	}
}
