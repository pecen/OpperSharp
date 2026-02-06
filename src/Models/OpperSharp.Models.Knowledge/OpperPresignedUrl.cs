using Newtonsoft.Json;

namespace OpperSharp.Models.Knowledge
{
	/// <summary>
	/// Presigned URL for file upload or download.
	/// </summary>
	public class OpperPresignedUrl
	{
		[JsonProperty("url")]
		public string Url { get; set; } = string.Empty;

		[JsonProperty("file_id")]
		public string? FileId { get; set; }

		[JsonProperty("expires_at")]
		public string? ExpiresAt { get; set; }
	}
}
