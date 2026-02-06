using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
		[JsonConverter(typeof(FlexibleDictionaryConverter))]
		public Dictionary<string, object>? Input { get; set; }

		[JsonProperty("output")]
		[JsonConverter(typeof(FlexibleDictionaryConverter))]
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

	/// <summary>
	/// Converter that handles JSON values that can be either a string or a dictionary.
	/// If it's a string, it tries to parse it as JSON.
	/// </summary>
	public class FlexibleDictionaryConverter : JsonConverter<Dictionary<string, object>?>
	{
		public override Dictionary<string, object>? ReadJson(
			JsonReader reader,
			Type objectType,
			Dictionary<string, object>? existingValue,
			bool hasExistingValue,
			JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
				return null;

			if (reader.TokenType == JsonToken.String)
			{
				// API returned a JSON string - parse it
				var jsonString = reader.Value?.ToString();
				if (string.IsNullOrWhiteSpace(jsonString))
					return null;

				try
				{
					var parsed = JToken.Parse(jsonString);
					return parsed.ToObject<Dictionary<string, object>>();
				}
				catch
				{
					// If parsing fails, return a dictionary with the string as-is
					return new Dictionary<string, object> { ["value"] = jsonString };
				}
			}

			if (reader.TokenType == JsonToken.StartObject)
			{
				// Normal dictionary
				var obj = JObject.Load(reader);
				return obj.ToObject<Dictionary<string, object>>();
			}

			return null;
		}

		public override void WriteJson(JsonWriter writer, Dictionary<string, object>? value, JsonSerializer serializer)
		{
			if (value == null)
			{
				writer.WriteNull();
			}
			else
			{
				serializer.Serialize(writer, value);
			}
		}
	}
}