using System;
using System.Collections.Generic;

namespace OpperSharp.Utilities
{
	/// <summary>
	/// Configuration options for OpperClient.
	/// </summary>
	public class OpperClientOptions
	{
		private static string _baseUrl = "https://api.opper.ai";

		/// <summary>
		/// Opper AI API key.
		/// </summary>
		public string ApiKey { get; set; } = string.Empty;

		/// <summary>
		/// Base URL for the API (default: https://api.opper.ai).
		/// </summary>
		public string BaseUrl { get; set; } = _baseUrl;

		/// <summary>
		/// HTTP request timeout (default: 120 seconds).
		/// </summary>
		public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(120);

		/// <summary>
		/// Maximum number of retries for transient failures (default: 3).
		/// </summary>
		public int MaxRetries { get; set; } = 3;

		/// <summary>
		/// Whether to enable automatic retries (default: true).
		/// </summary>
		public bool EnableRetries { get; set; } = true;

		/// <summary>
		/// Default model to use for operations (optional).
		/// </summary>
		public string? DefaultModel { get; set; }

		/// <summary>
		/// Custom HTTP headers to include in all requests.
		/// </summary>
		public Dictionary<string, string>? CustomHeaders { get; set; }

		/// <summary>
		/// Creates options from environment variables.
		/// </summary>
		public static OpperClientOptions FromEnvironment()
		{
			return new OpperClientOptions
			{
				ApiKey = Environment.GetEnvironmentVariable("OPPER_API_KEY") ?? string.Empty,
				BaseUrl = Environment.GetEnvironmentVariable("OPPER_BASE_URL") ?? "https://api.opper.ai"
			};
		}
	}
}
