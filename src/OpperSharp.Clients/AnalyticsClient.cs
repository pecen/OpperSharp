using Newtonsoft.Json;
using OpperSharp.Exceptions;
using OpperSharp.Models.Analytics;
using OpperSharp.Utilities.Enums;
using OpperSharp.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Task;

namespace OpperSharp.Clients
{
	/// <summary>
	/// Client for Opper AI analytics operations (v2 API).
	/// </summary>
	public class AnalyticsClient
	{
		private readonly HttpClient _httpClient;
		private readonly string _analyticsEndpoint;

		public AnalyticsClient(HttpClient httpClient)
		{
			_httpClient = httpClient;
			_analyticsEndpoint = EndPoints.AnalyticsUsage.GetDescription();
		}

		/// <summary>
		/// Get usage analytics.
		/// </summary>
		public async Task<OpperAnalyticsResponse> GetUsageAsync(
			DateTime? fromDate = null,
			DateTime? toDate = null,
			string? granularity = null,
			List<string>? fields = null,
			List<string>? groupBy = null,
			CancellationToken cancellationToken = default)
		{
			var queryParams = new List<string>();

			if (fromDate.HasValue)
				queryParams.Add($"from_date={fromDate.Value:yyyy-MM-ddTHH:mm:ss}");

			if (toDate.HasValue)
				queryParams.Add($"to_date={toDate.Value:yyyy-MM-ddTHH:mm:ss}");

			if (granularity != null)
				queryParams.Add($"granularity={granularity}");

			if (fields != null && fields.Count > 0)
				queryParams.Add($"fields={string.Join(",", fields)}");

			if (groupBy != null && groupBy.Count > 0)
				queryParams.Add($"group_by={string.Join(",", groupBy)}");

			var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";

			var response = await _httpClient.GetAsync($"{_analyticsEndpoint}{queryString}", cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to get analytics: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			return JsonConvert.DeserializeObject<OpperAnalyticsResponse>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}
	}
}
