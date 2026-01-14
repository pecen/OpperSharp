using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace OpperSharp.Models.Spans
{
	/// <summary>
	/// Represents a span for distributed tracing.
	/// </summary>
	public class OpperSpan
	{
		[JsonProperty("id")]
		public string Id { get; set; } = string.Empty;

		[JsonProperty("trace_id")]
		public string? TraceId { get; set; }

		[JsonProperty("parent_span_id")]
		public string? ParentSpanId { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; } = string.Empty;

		[JsonProperty("input")]
		public Dictionary<string, object>? Input { get; set; }

		[JsonProperty("output")]
		public Dictionary<string, object>? Output { get; set; }

		[JsonProperty("status")]
		public string? Status { get; set; }

		[JsonProperty("error")]
		public string? Error { get; set; }

		[JsonProperty("metadata")]
		public Dictionary<string, object>? Metadata { get; set; }

		[JsonProperty("start_time")]
		public DateTime? StartTime { get; set; }

		[JsonProperty("end_time")]
		public DateTime? EndTime { get; set; }

		[JsonProperty("duration_ms")]
		public long? DurationMs { get; set; }
	}
}
