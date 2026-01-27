using Newtonsoft.Json;
using System.Collections.Generic;

namespace OpperSharp.Models.Knowledge
{
	/// <summary>
	/// Request for registering a file upload to a knowledge base.
	/// </summary>
	public class OpperFileUploadRequest
	{
		[JsonProperty("filename")]
		public string Filename { get; set; } = string.Empty;

		[JsonProperty("file_id")]
		public string FileId { get; set; } = string.Empty;

		[JsonProperty("content_type")]
		public string ContentType { get; set; } = string.Empty;

		[JsonProperty("configuration")]
		public Dictionary<string, object>? Configuration { get; set; }

		[JsonProperty("metadata")]
		public Dictionary<string, object>? Metadata { get; set; }
	}
}
