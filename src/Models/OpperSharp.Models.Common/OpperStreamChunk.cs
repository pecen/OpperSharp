using Newtonsoft.Json;

namespace OpperSharp.Models.Common
{
	/// <summary>
	/// Represents a chunk of streamed response data.
	/// </summary>
	public class OpperStreamChunk
	{
		[JsonProperty("delta")]
		public string? Delta { get; set; }

		[JsonProperty("span_id")]
		public string? SpanId { get; set; }

		[JsonProperty("trace_id")]
		public string? TraceId { get; set; }

		[JsonProperty("done")]
		public bool Done { get; set; }
	}
}
