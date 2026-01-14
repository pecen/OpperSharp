using Newtonsoft.Json;
using OpperSharp.Exceptions;
using OpperSharp.Models.Common;
using OpperSharp.Models.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JsonException = Newtonsoft.Json.JsonException;

namespace OpperSharp.Clients
{
	/// <summary>
	/// Client for Opper AI function operations.
	/// </summary>
	public class FunctionsClient
	{
		private readonly HttpClient _httpClient;

		public FunctionsClient(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		/// <summary>
		/// Call a function with the given input.
		/// </summary>
		public async Task<OpperFunctionResponse> CallAsync(
			string path,
			Dictionary<string, object> input,
			OpperCallOptions? options = null,
			CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(path))
				throw new ArgumentException("Function path cannot be null or empty", nameof(path));

			options ??= new OpperCallOptions();

			var requestBody = new Dictionary<string, object>
			{
				["input"] = input
			};

			if (options.Context != null)
				requestBody["context"] = options.Context;

			if (options.ParentSpanId != null)
				requestBody["parent_span_id"] = options.ParentSpanId;

			if (options.Cache.HasValue)
				requestBody["cache"] = options.Cache.Value;

			if (options.Model != null)
				requestBody["model"] = options.Model;

			if (options.Temperature.HasValue)
				requestBody["temperature"] = options.Temperature.Value;

			if (options.Metadata != null)
				requestBody["metadata"] = options.Metadata;

			var content = new StringContent(
				JsonConvert.SerializeObject(requestBody),
				Encoding.UTF8,
				"application/json"
			);

			var response = await _httpClient.PostAsync(
				$"/v1/call/{path}",
				content,
				cancellationToken
			);

			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Function call failed: {response.StatusCode}",
					responseString,
					(int)response.StatusCode,
					$"/v1/call/{path}"
				);
			}

			return JsonConvert.DeserializeObject<OpperFunctionResponse>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// Call a function with streaming response.
		/// </summary>
		public async IAsyncEnumerable<OpperStreamChunk> CallStreamAsync(
			string path,
			Dictionary<string, object> input,
			OpperCallOptions? options = null,
			[EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(path))
				throw new ArgumentException("Function path cannot be null or empty", nameof(path));

			options ??= new OpperCallOptions();

			var requestBody = new Dictionary<string, object>
			{
				["input"] = input,
				["stream"] = true
			};

			if (options.Context != null)
				requestBody["context"] = options.Context;

			if (options.ParentSpanId != null)
				requestBody["parent_span_id"] = options.ParentSpanId;

			var content = new StringContent(
				JsonConvert.SerializeObject(requestBody),
				Encoding.UTF8,
				"application/json"
			);

			var request = new HttpRequestMessage(HttpMethod.Post, $"/v1/call/{path}")
			{
				Content = content
			};

			using var response = await _httpClient.SendAsync(
				request,
				HttpCompletionOption.ResponseHeadersRead,
				cancellationToken
			);

			if (!response.IsSuccessStatusCode)
			{
				var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
				throw new OpperAPIException(
					$"Function stream call failed: {response.StatusCode}",
					errorContent,
					(int)response.StatusCode
				);
			}

			using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
			using var reader = new StreamReader(stream);

			while (!reader.EndOfStream)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var line = await reader.ReadLineAsync();

				if (string.IsNullOrWhiteSpace(line))
					continue;

				if (!line.StartsWith("data: "))
					continue;

				var jsonData = line[6..];

				if (jsonData == "[DONE]")
					break;

				OpperStreamChunk? chunk;
				try
				{
					chunk = JsonConvert.DeserializeObject<OpperStreamChunk>(jsonData);
				}
				catch (JsonException)
				{
					continue;
				}

				if (chunk != null)
					yield return chunk;
			}
		}

		/// <summary>
		/// Create a new function.
		/// </summary>
		public async Task<OpperFunction> CreateAsync(
			OpperFunctionDefinition definition,
			CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(definition.Path))
				throw new ArgumentException("Function path is required", nameof(definition));

			var content = new StringContent(
				JsonConvert.SerializeObject(definition, new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				}),
				Encoding.UTF8,
				"application/json"
			);

			var response = await _httpClient.PostAsync("/v1/functions", content, cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to create function: {response.StatusCode}",
					responseString,
					(int)response.StatusCode,
					"/v1/functions"
				);
			}

			return JsonConvert.DeserializeObject<OpperFunction>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// Update an existing function.
		/// </summary>
		public async Task<OpperFunction> UpdateAsync(
			string path,
			OpperFunctionDefinition definition,
			CancellationToken cancellationToken = default)
		{
			var content = new StringContent(
				JsonConvert.SerializeObject(definition, new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				}),
				Encoding.UTF8,
				"application/json"
			);

			var response = await _httpClient.PutAsync(
				$"/v1/functions/{path}",
				content,
				cancellationToken
			);

			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to update function: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			return JsonConvert.DeserializeObject<OpperFunction>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// Get a function by path.
		/// </summary>
		public async Task<OpperFunction> GetAsync(
			string path,
			CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.GetAsync($"/v1/functions/{path}", cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to get function: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			return JsonConvert.DeserializeObject<OpperFunction>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// List all functions.
		/// </summary>
		public async Task<List<OpperFunction>> ListAsync(CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.GetAsync("/v1/functions", cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to list functions: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			var result = JsonConvert.DeserializeObject<OpperListResponse<OpperFunction>>(responseString);
			return result?.Data ?? new List<OpperFunction>();
		}

		/// <summary>
		/// Delete a function by path.
		/// </summary>
		public async Task DeleteAsync(string path, CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.DeleteAsync($"/v1/functions/{path}", cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
				throw new OpperAPIException(
					$"Failed to delete function: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}
		}

		/// <summary>
		/// Check if a function exists.
		/// </summary>
		public async Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
		{
			try
			{
				await GetAsync(path, cancellationToken);
				return true;
			}
			catch (OpperAPIException ex) when (ex.StatusCode == 404)
			{
				return false;
			}
		}
	}
}
