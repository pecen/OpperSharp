using Newtonsoft.Json;
using OpperSharp.Exceptions;
using OpperSharp.Models.Common;
using OpperSharp.Models.Datasets;
using OpperSharp.Utilities.Enums;
using OpperSharp.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpperSharp.Clients
{
	/// <summary>
	/// Client for Opper AI dataset operations (v2 API).
	/// </summary>
	public class DatasetsClient
	{
		private readonly HttpClient _httpClient;
		private readonly string _datasetsEndpoint;

		public DatasetsClient(HttpClient httpClient)
		{
			_httpClient = httpClient;
			_datasetsEndpoint = EndPoints.Datasets.GetDescription();
		}

		/// <summary>
		/// Create a new dataset entry.
		/// </summary>
		public async Task<OpperDatasetEntry> CreateEntryAsync(
			string datasetId,
			object input,
			object output,
			object? expected = null,
			string? comment = null,
			CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(datasetId))
				throw new ArgumentException("Dataset ID cannot be null or empty", nameof(datasetId));

			var requestBody = new Dictionary<string, object>
			{
				["input"] = input,
				["output"] = output
			};

			if (expected != null)
				requestBody["expected"] = expected;

			if (comment != null)
				requestBody["comment"] = comment;

			var content = new StringContent(
				JsonConvert.SerializeObject(requestBody),
				Encoding.UTF8,
				"application/json"
			);

			var response = await _httpClient.PostAsync(
				$"{_datasetsEndpoint}/{datasetId}",
				content,
				cancellationToken
			);

			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to create dataset entry: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			return JsonConvert.DeserializeObject<OpperDatasetEntry>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// List dataset entries (paginated).
		/// </summary>
		public async Task<List<OpperDatasetEntry>> ListEntriesAsync(
			string datasetId,
			int offset = 0,
			int limit = 100,
			CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.GetAsync(
				$"{_datasetsEndpoint}/{datasetId}/entries?offset={offset}&limit={limit}",
				cancellationToken
			);

			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to list dataset entries: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			var result = JsonConvert.DeserializeObject<OpperListResponse<OpperDatasetEntry>>(responseString);
			return result?.Data ?? new List<OpperDatasetEntry>();
		}

		/// <summary>
		/// Get a specific dataset entry.
		/// </summary>
		public async Task<OpperDatasetEntry> GetEntryAsync(
			string datasetId,
			string entryId,
			CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.GetAsync(
				$"{_datasetsEndpoint}/{datasetId}/entries/{entryId}",
				cancellationToken
			);

			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to get dataset entry: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			return JsonConvert.DeserializeObject<OpperDatasetEntry>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// Delete a dataset entry.
		/// </summary>
		public async Task DeleteEntryAsync(
			string datasetId,
			string entryId,
			CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.DeleteAsync(
				$"{_datasetsEndpoint}/{datasetId}/entries/{entryId}",
				cancellationToken
			);

			if (!response.IsSuccessStatusCode)
			{
				var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
				throw new OpperAPIException(
					$"Failed to delete dataset entry: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}
		}

		/// <summary>
		/// Query dataset entries.
		/// </summary>
		public async Task<List<OpperDatasetEntry>> QueryEntriesAsync(
			string datasetId,
			string query,
			int limit = 5,
			CancellationToken cancellationToken = default)
		{
			var requestBody = new OpperDatasetQueryRequest
			{
				Query = query,
				Limit = limit
			};

			var content = new StringContent(
				JsonConvert.SerializeObject(requestBody),
				Encoding.UTF8,
				"application/json"
			);

			var response = await _httpClient.PostAsync(
				$"{_datasetsEndpoint}/{datasetId}/entries/query",
				content,
				cancellationToken
			);

			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to query dataset entries: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			var result = JsonConvert.DeserializeObject<OpperListResponse<OpperDatasetEntry>>(responseString);
			return result?.Data ?? new List<OpperDatasetEntry>();
		}
	}
}
