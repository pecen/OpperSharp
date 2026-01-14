using Newtonsoft.Json;
using OpperSharp.Exceptions;
using OpperSharp.Models.Common;
using OpperSharp.Models.Indexes;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpperSharp.Clients
{
	/// <summary>
	/// Client for Opper AI index operations.
	/// </summary>
	public class IndexesClient
	{
		private readonly HttpClient _httpClient;

		public IndexesClient(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		/// <summary>
		/// Create a new index.
		/// </summary>
		public async Task<OpperIndex> CreateAsync(
			string name,
			string? description = null,
			CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("Index name cannot be null or empty", nameof(name));

			var requestBody = new Dictionary<string, object>
			{
				["name"] = name
			};

			if (description != null)
				requestBody["description"] = description;

			var content = new StringContent(
				JsonConvert.SerializeObject(requestBody),
				Encoding.UTF8,
				"application/json"
			);

			var response = await _httpClient.PostAsync("/v1/indexes", content, cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to create index: {response.StatusCode}",
					responseString,
					(int)response.StatusCode,
					"/v1/indexes"
				);
			}

			return JsonConvert.DeserializeObject<OpperIndex>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// Get an index by name.
		/// </summary>
		public async Task<OpperIndex> GetAsync(
			string name,
			CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.GetAsync($"/v1/indexes/{name}", cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to get index: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			return JsonConvert.DeserializeObject<OpperIndex>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// List all indexes.
		/// </summary>
		public async Task<List<OpperIndex>> ListAsync(CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.GetAsync("/v1/indexes", cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to list indexes: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			var result = JsonConvert.DeserializeObject<OpperListResponse<OpperIndex>>(responseString);
			return result?.Data ?? new List<OpperIndex>();
		}

		/// <summary>
		/// Delete an index by name.
		/// </summary>
		public async Task DeleteAsync(string name, CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.DeleteAsync($"/v1/indexes/{name}", cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
				throw new OpperAPIException(
					$"Failed to delete index: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}
		}

		/// <summary>
		/// Add a document to an index.
		/// </summary>
		public async Task<OpperIndexedDocument> AddAsync(
			string indexName,
			string content,
			Dictionary<string, object>? metadata = null,
			string? id = null,
			CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(indexName))
				throw new ArgumentException("Index name cannot be null or empty", nameof(indexName));

			if (string.IsNullOrWhiteSpace(content))
				throw new ArgumentException("Content cannot be null or empty", nameof(content));

			var requestBody = new Dictionary<string, object>
			{
				["content"] = content
			};

			if (metadata != null)
				requestBody["metadata"] = metadata;

			if (id != null)
				requestBody["id"] = id;

			var jsonContent = new StringContent(
				JsonConvert.SerializeObject(requestBody),
				Encoding.UTF8,
				"application/json"
			);

			var response = await _httpClient.PostAsync(
				$"/v1/indexes/{indexName}/index",
				jsonContent,
				cancellationToken
			);

			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to add document: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			return JsonConvert.DeserializeObject<OpperIndexedDocument>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// Add multiple documents to an index in bulk.
		/// </summary>
		public async Task<List<OpperIndexedDocument>> AddBulkAsync(
			string indexName,
			List<OpperDocument> documents,
			CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(indexName))
				throw new ArgumentException("Index name cannot be null or empty", nameof(indexName));

			if (documents == null || documents.Count == 0)
				throw new ArgumentException("Documents list cannot be null or empty", nameof(documents));

			var requestBody = new { documents };

			var content = new StringContent(
				JsonConvert.SerializeObject(requestBody),
				Encoding.UTF8,
				"application/json"
			);

			var response = await _httpClient.PostAsync(
				$"/v1/indexes/{indexName}/index/bulk",
				content,
				cancellationToken
			);

			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to add documents in bulk: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			var result = JsonConvert.DeserializeObject<OpperListResponse<OpperIndexedDocument>>(responseString);
			return result?.Data ?? new List<OpperIndexedDocument>();
		}

		/// <summary>
		/// Query an index for similar documents.
		/// </summary>
		public async Task<OpperSearchResponse> QueryAsync(
			string indexName,
			string query,
			int k = 10,
			Dictionary<string, object>? filters = null,
			CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(indexName))
				throw new ArgumentException("Index name cannot be null or empty", nameof(indexName));

			if (string.IsNullOrWhiteSpace(query))
				throw new ArgumentException("Query cannot be null or empty", nameof(query));

			var requestBody = new Dictionary<string, object>
			{
				["query"] = query,
				["k"] = k
			};

			if (filters != null)
				requestBody["filters"] = filters;

			var content = new StringContent(
				JsonConvert.SerializeObject(requestBody),
				Encoding.UTF8,
				"application/json"
			);

			var response = await _httpClient.PostAsync(
				$"/v1/indexes/{indexName}/query",
				content,
				cancellationToken
			);

			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Query failed: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			return JsonConvert.DeserializeObject<OpperSearchResponse>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// Retrieve a specific document by ID.
		/// </summary>
		public async Task<OpperIndexedDocument> RetrieveAsync(
			string indexName,
			string documentId,
			CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.GetAsync(
				$"/v1/indexes/{indexName}/documents/{documentId}",
				cancellationToken
			);

			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to retrieve document: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			return JsonConvert.DeserializeObject<OpperIndexedDocument>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// Delete a specific document from an index.
		/// </summary>
		public async Task DeleteDocumentAsync(
			string indexName,
			string documentId,
			CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.DeleteAsync(
				$"/v1/indexes/{indexName}/documents/{documentId}",
				cancellationToken
			);

			if (!response.IsSuccessStatusCode)
			{
				var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
				throw new OpperAPIException(
					$"Failed to delete document: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}
		}

		/// <summary>
		/// Check if an index exists.
		/// </summary>
		public async Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default)
		{
			try
			{
				await GetAsync(name, cancellationToken);
				return true;
			}
			catch (OpperAPIException ex) when (ex.StatusCode == 404)
			{
				return false;
			}
		}

		/// <summary>
		/// Get or create an index (creates if it doesn't exist).
		/// </summary>
		public async Task<OpperIndex> GetOrCreateAsync(
			string name,
			string? description = null,
			CancellationToken cancellationToken = default)
		{
			try
			{
				return await GetAsync(name, cancellationToken);
			}
			catch (OpperAPIException ex) when (ex.StatusCode == 404)
			{
				return await CreateAsync(name, description, cancellationToken);
			}
		}
	}
}
