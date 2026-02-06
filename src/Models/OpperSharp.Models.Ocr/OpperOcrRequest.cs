using Newtonsoft.Json;
using System.Collections.Generic;

namespace OpperSharp.Models.Ocr
{
	public class OpperOcrRequest
	{
		[JsonProperty("model")]
		public string Model { get; set; } = string.Empty;

		[JsonProperty("document")]
		public object Document { get; set; } = new();

		[JsonProperty("pages")]
		public List<int>? Pages { get; set; }

		[JsonProperty("include_image_base64")]
		public bool? IncludeImageBase64 { get; set; }
	}

	public class OpperOcrResponse
	{
		[JsonProperty("text")]
		public string Text { get; set; } = string.Empty;

		[JsonProperty("pages")]
		public List<OpperOcrPage>? Pages { get; set; }
	}

	public class OpperOcrPage
	{
		[JsonProperty("page_number")]
		public int PageNumber { get; set; }

		[JsonProperty("text")]
		public string Text { get; set; } = string.Empty;

		[JsonProperty("image_base64")]
		public string? ImageBase64 { get; set; }
	}
}
