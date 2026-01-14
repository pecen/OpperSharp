using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace OpperSharp.Models.Functions
{
	/// <summary>
	/// Response from a function call.
	/// </summary>
	public class OpperFunctionResponse
	{
		[JsonProperty("output")]
		public JObject Output { get; set; } = new();

		[JsonProperty("message")]
		public string? Message { get; set; }

		[JsonProperty("span_id")]
		public string? SpanId { get; set; }

		[JsonProperty("trace_id")]
		public string? TraceId { get; set; }

		[JsonProperty("cached")]
		public bool Cached { get; set; }

		[JsonProperty("usage")]
		public OpperFunctionUsage? Usage { get; set; }

		/// <summary>
		/// Deserialize the output to a specific type.
		/// </summary>
		public T GetOutput<T>()
		{
			return Output.ToObject<T>()
				?? throw new InvalidOperationException($"Failed to deserialize output to {typeof(T).Name}");
		}

		/// <summary>
		/// Try to get the output as a specific type.
		/// </summary>
		public bool TryGetOutput<T>(out T? result)
		{
			try
			{
				result = Output.ToObject<T>();
				return result != null;
			}
			catch
			{
				result = default;
				return false;
			}
		}
	}

	/// <summary>
	/// Token usage information for a function call.
	/// </summary>
	public class OpperFunctionUsage
	{
		[JsonProperty("prompt_tokens")]
		public int PromptTokens { get; set; }

		[JsonProperty("completion_tokens")]
		public int CompletionTokens { get; set; }

		[JsonProperty("total_tokens")]
		public int TotalTokens { get; set; }
	}
}
