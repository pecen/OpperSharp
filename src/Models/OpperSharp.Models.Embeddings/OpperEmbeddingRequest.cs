using Newtonsoft.Json;

namespace OpperSharp.Models.Embeddings
{
	/// <summary>
	/// Request for creating embeddings.
	/// </summary>
	public class OpperEmbeddingRequest
	{
		[JsonProperty("input")]
		public object Input { get; set; } = string.Empty;

		[JsonProperty("model")]
		public string? Model { get; set; }
	}
}
