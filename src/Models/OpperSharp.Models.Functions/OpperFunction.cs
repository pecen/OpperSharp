using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace OpperSharp.Models.Functions
{
	/// <summary>
	/// Represents an Opper AI function.
	/// </summary>
	public class OpperFunction
	{
		[JsonProperty("id")]
		public string Id { get; set; } = string.Empty;

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
		[JsonConverter(typeof(FlexibleModelConverter))]
		public string? Model { get; set; }

		[JsonProperty("index_ids")]
		public List<string>? IndexIds { get; set; }

		[JsonProperty("created_at")]
		public DateTime? CreatedAt { get; set; }

		[JsonProperty("updated_at")]
		public DateTime? UpdatedAt { get; set; }
	}

	/// <summary>
	/// Custom JSON converter that handles both string and array for the model field.
	/// Opper API v2 can return model as either a string or an array of strings (fallback chain).
	/// </summary>
	public class FlexibleModelConverter : JsonConverter<string?>
	{
		public override string? ReadJson(JsonReader reader, Type objectType, string? existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
				return null;

			if (reader.TokenType == JsonToken.String)
				return reader.Value?.ToString();

			if (reader.TokenType == JsonToken.StartArray)
			{
				var array = JArray.Load(reader);
				if (array.Count > 0)
					return array[0].ToString(); // Return first model in fallback chain
				return null;
			}

			return null;
		}

		public override void WriteJson(JsonWriter writer, string? value, JsonSerializer serializer)
		{
			if (value == null)
				writer.WriteNull();
			else
				writer.WriteValue(value);
		}
	}
}
