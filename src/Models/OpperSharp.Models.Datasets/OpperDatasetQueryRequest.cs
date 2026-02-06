using Newtonsoft.Json;

namespace OpperSharp.Models.Datasets
{
	/// <summary>
	/// Request for querying dataset entries.
	/// </summary>
	public class OpperDatasetQueryRequest
	{
		[JsonProperty("query")]
		public string Query { get; set; } = string.Empty;

		[JsonProperty("limit")]
		public int Limit { get; set; } = 5;
	}
}
