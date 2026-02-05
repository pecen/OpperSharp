using Newtonsoft.Json;
using OpperSharp.Exceptions;
using OpperSharp.Models.Common;
using OpperSharp.Models.Functions;
using OpperSharp.Utilities.Enums;
using OpperSharp.Utilities.Extensions;
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
		private readonly string _callEndpoint;
		private readonly string _functionsEndpoint;

		public FunctionsClient(HttpClient httpClient)
		{
			_httpClient = httpClient;

			_callEndpoint = _httpClient.BaseAddress + EndPoints.Calls.GetDescription();
			_functionsEndpoint = _httpClient.BaseAddress + EndPoints.Functions.GetDescription();
		}

		/// <summary>
		/// Call a function with the given input.
		/// Supports both named functions (with path) and ad-hoc calls (with options.Name and options.Instructions).
		/// </summary>
		public async Task<OpperFunctionResponse> CallAsync(
			string? path,
			Dictionary<string, object> input,
			OpperCallOptions? options = null,
			CancellationToken cancellationToken = default)
		{
			options ??= new OpperCallOptions();

			// Determine if this is an ad-hoc call or a named function call
			bool isAdHocCall = string.IsNullOrWhiteSpace(path) && !string.IsNullOrWhiteSpace(options.Name);

			var requestBody = new Dictionary<string, object>
			{
				["input"] = input
			};

			// For ad-hoc calls, include name and instructions in the request body
			if (isAdHocCall)
			{
				requestBody["name"] = options.Name!;

				if (!string.IsNullOrWhiteSpace(options.Instructions))
					requestBody["instructions"] = options.Instructions;
			}
			else if (string.IsNullOrWhiteSpace(path))
			{
				throw new ArgumentException(
					"Either path must be provided (for named function calls) or options.Name must be set (for ad-hoc calls)",
					nameof(path));
			}

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

			if (options.Tools != null && options.Tools.Count > 0)
				requestBody["tools"] = options.Tools;

			var jsonBody = JsonConvert.SerializeObject(requestBody);

			// DEBUG: Log request for ad-hoc calls
			if (isAdHocCall)
			{
				System.Console.WriteLine($"[DEBUG] Ad-hoc request body: {jsonBody.Substring(0, Math.Min(500, jsonBody.Length))}...");
			}

			var content = new StringContent(
				jsonBody,
				Encoding.UTF8,
				"application/json"
			);

			// Choose endpoint based on call type
			string endpoint = isAdHocCall
				? _callEndpoint  // POST /v2/call (ad-hoc)
				: $"{_callEndpoint}/{path}";  // POST /v2/call/{path} (named function)

			var response = await _httpClient.PostAsync(
				endpoint,
				content,
				cancellationToken
			);

			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			// DEBUG: Log response for ad-hoc calls
			if (isAdHocCall)
			{
				System.Console.WriteLine($"[DEBUG] Ad-hoc response status: {response.StatusCode}");
				System.Console.WriteLine($"[DEBUG] Ad-hoc response body: {responseString.Substring(0, Math.Min(500, responseString.Length))}...");
			}

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Function call failed: {response.StatusCode}",
					responseString,
					(int)response.StatusCode,
					endpoint
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

			if (options.Model != null)
				requestBody["model"] = options.Model;

			if (options.Temperature.HasValue)
				requestBody["temperature"] = options.Temperature.Value;

			if (options.Metadata != null)
				requestBody["metadata"] = options.Metadata;

			if (options.Tools != null && options.Tools.Count > 0)
				requestBody["tools"] = options.Tools;

			var content = new StringContent(
				JsonConvert.SerializeObject(requestBody),
				Encoding.UTF8,
				"application/json"
			);

			var request = new HttpRequestMessage(HttpMethod.Post, $"{_callEndpoint}/{path}")
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

			var response = await _httpClient.PostAsync(_functionsEndpoint, content, cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to create function: {response.StatusCode}",
					responseString,
					(int)response.StatusCode,
					_functionsEndpoint
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
				$"{_functionsEndpoint}/{path}",
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
			var response = await _httpClient.GetAsync($"{_functionsEndpoint}/{path}", cancellationToken);
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
			var response = await _httpClient.GetAsync(_functionsEndpoint, cancellationToken);
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
			var response = await _httpClient.DeleteAsync($"{_functionsEndpoint}/{path}", cancellationToken);

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
