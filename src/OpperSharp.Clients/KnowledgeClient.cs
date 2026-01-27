using Newtonsoft.Json;
using OpperSharp.Exceptions;
using OpperSharp.Models.Common;
using OpperSharp.Models.Knowledge;
using OpperSharp.Utilities.Enums;
using OpperSharp.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpperSharp.Clients
{
	/// <summary>
	/// Client for Opper AI knowledge base operations (v2 API).
	/// </summary>
	public class KnowledgeClient
	{
		private readonly HttpClient _httpClient;
		private readonly string _knowledgeEndpoint;

		public KnowledgeClient(HttpClient httpClient)
		{
			_httpClient = httpClient;
			_knowledgeEndpoint = EndPoints.Knowledge.GetDescription();
		}

		/// <summary>
		/// Create a new knowledge base.
		/// </summary>
		public async Task<OpperKnowledgeBase> CreateAsync(
			string name,
			string? embeddingModel = null,
			CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("Knowledge base name cannot be null or empty", nameof(name));

			var requestBody = new Dictionary<string, object>
			{
				["name"] = name
			};

			if (embeddingModel != null)
				requestBody["embedding_model"] = embeddingModel;

			var content = new StringContent(
				JsonConvert.SerializeObject(requestBody),
				Encoding.UTF8,
				"application/json"
			);

			var response = await _httpClient.PostAsync(_knowledgeEndpoint, content, cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to create knowledge base: {response.StatusCode}",
					responseString,
					(int)response.StatusCode,
					_knowledgeEndpoint
				);
			}

			return JsonConvert.DeserializeObject<OpperKnowledgeBase>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// Get a knowledge base by ID.
		/// </summary>
		public async Task<OpperKnowledgeBase> GetAsync(
			string knowledgeBaseId,
			CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.GetAsync($"{_knowledgeEndpoint}/{knowledgeBaseId}", cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to get knowledge base: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			return JsonConvert.DeserializeObject<OpperKnowledgeBase>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// Get a knowledge base by name.
		/// </summary>
		public async Task<OpperKnowledgeBase> GetByNameAsync(
			string name,
			CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.GetAsync($"{_knowledgeEndpoint}/by-name/{name}", cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to get knowledge base by name: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			return JsonConvert.DeserializeObject<OpperKnowledgeBase>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// List all knowledge bases (paginated).
		/// </summary>
		public async Task<List<OpperKnowledgeBase>> ListAsync(
			int offset = 0,
			int limit = 100,
			CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.GetAsync(
				$"{_knowledgeEndpoint}?offset={offset}&limit={limit}",
				cancellationToken
			);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to list knowledge bases: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			var result = JsonConvert.DeserializeObject<OpperListResponse<OpperKnowledgeBase>>(responseString);
			return result?.Data ?? new List<OpperKnowledgeBase>();
		}

		/// <summary>
		/// Delete a knowledge base by ID.
		/// </summary>
		public async Task DeleteAsync(string knowledgeBaseId, CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.DeleteAsync($"{_knowledgeEndpoint}/{knowledgeBaseId}", cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
				throw new OpperAPIException(
					$"Failed to delete knowledge base: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}
		}

		/// <summary>
		/// Get a presigned URL for uploading a file.
		/// </summary>
		public async Task<OpperPresignedUrl> GetUploadUrlAsync(
			string knowledgeBaseId,
			CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.GetAsync(
				$"{_knowledgeEndpoint}/{knowledgeBaseId}/upload_url",
				cancellationToken
			);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to get upload URL: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			return JsonConvert.DeserializeObject<OpperPresignedUrl>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// Register a file upload after uploading to presigned URL.
		/// </summary>
		public async Task<OpperKnowledgeFile> RegisterFileAsync(
			string knowledgeBaseId,
			OpperFileUploadRequest request,
			CancellationToken cancellationToken = default)
		{
			var content = new StringContent(
				JsonConvert.SerializeObject(request),
				Encoding.UTF8,
				"application/json"
			);

			var response = await _httpClient.PostAsync(
				$"{_knowledgeEndpoint}/{knowledgeBaseId}/register_file",
				content,
				cancellationToken
			);

			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to register file: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			return JsonConvert.DeserializeObject<OpperKnowledgeFile>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// Upload a file directly to the knowledge base.
		/// </summary>
		public async Task<OpperKnowledgeFile> UploadFileAsync(
			string knowledgeBaseId,
			string filename,
			Stream fileStream,
			string contentType = "application/octet-stream",
			Dictionary<string, object>? metadata = null,
			Dictionary<string, object>? configuration = null,
			CancellationToken cancellationToken = default)
		{
			using var multipartContent = new MultipartFormDataContent();
			using var streamContent = new StreamContent(fileStream);

			streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
			multipartContent.Add(streamContent, "file", filename);

			if (metadata != null)
			{
				multipartContent.Add(
					new StringContent(JsonConvert.SerializeObject(metadata)),
					"metadata"
				);
			}

			if (configuration != null)
			{
				multipartContent.Add(
					new StringContent(JsonConvert.SerializeObject(configuration)),
					"configuration"
				);
			}

			var response = await _httpClient.PostAsync(
				$"{_knowledgeEndpoint}/{knowledgeBaseId}/upload",
				multipartContent,
				cancellationToken
			);

			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to upload file: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			return JsonConvert.DeserializeObject<OpperKnowledgeFile>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// List files in a knowledge base (paginated).
		/// </summary>
		public async Task<List<OpperKnowledgeFile>> ListFilesAsync(
			string knowledgeBaseId,
			int offset = 0,
			int limit = 100,
			CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.GetAsync(
				$"{_knowledgeEndpoint}/{knowledgeBaseId}/files?offset={offset}&limit={limit}",
				cancellationToken
			);

			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to list files: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			var result = JsonConvert.DeserializeObject<OpperListResponse<OpperKnowledgeFile>>(responseString);
			return result?.Data ?? new List<OpperKnowledgeFile>();
		}

		/// <summary>
		/// Delete a file from a knowledge base.
		/// </summary>
		public async Task DeleteFileAsync(
			string knowledgeBaseId,
			string fileId,
			CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.DeleteAsync(
				$"{_knowledgeEndpoint}/{knowledgeBaseId}/files/{fileId}",
				cancellationToken
			);

			if (!response.IsSuccessStatusCode)
			{
				var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
				throw new OpperAPIException(
					$"Failed to delete file: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}
		}

		/// <summary>
		/// Get a presigned URL for downloading a file.
		/// </summary>
		public async Task<OpperPresignedUrl> GetDownloadUrlAsync(
			string knowledgeBaseId,
			string fileId,
			CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.GetAsync(
				$"{_knowledgeEndpoint}/{knowledgeBaseId}/files/{fileId}/download_url",
				cancellationToken
			);

			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to get download URL: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			return JsonConvert.DeserializeObject<OpperPresignedUrl>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// Check if a knowledge base exists.
		/// </summary>
		public async Task<bool> ExistsAsync(string knowledgeBaseId, CancellationToken cancellationToken = default)
		{
			try
			{
				await GetAsync(knowledgeBaseId, cancellationToken);
				return true;
			}
			catch (OpperAPIException ex) when (ex.StatusCode == 404)
			{
				return false;
			}
		}

		/// <summary>
		/// Get or create a knowledge base (creates if it doesn't exist).
		/// </summary>
		public async Task<OpperKnowledgeBase> GetOrCreateAsync(
			string name,
			string? embeddingModel = null,
			CancellationToken cancellationToken = default)
		{
			try
			{
				return await GetByNameAsync(name, cancellationToken);
			}
			catch (OpperAPIException ex) when (ex.StatusCode == 404)
			{
				return await CreateAsync(name, embeddingModel, cancellationToken);
			}
		}
	}
}
