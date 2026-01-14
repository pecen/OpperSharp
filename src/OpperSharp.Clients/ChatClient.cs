using Newtonsoft.Json;
using OpperSharp.Exceptions;
using OpperSharp.Models.Chat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpperSharp.Clients
{
	/// <summary>
	/// Client for Opper AI chat completions.
	/// </summary>
	public class ChatClient
	{
		private readonly HttpClient _httpClient;

		public ChatClient(HttpClient httpClient)
		{
			_httpClient = httpClient;
		}

		/// <summary>
		/// Create a chat completion.
		/// </summary>
		public async Task<OpperChatResponse> CompletionsAsync(
			List<OpperMessage> messages,
			string? model = null,
			double? temperature = null,
			int? maxTokens = null,
			Dictionary<string, object>? context = null,
			CancellationToken cancellationToken = default)
		{
			if (messages == null || messages.Count == 0)
				throw new ArgumentException("Messages list cannot be null or empty", nameof(messages));

			var requestBody = new Dictionary<string, object>
			{
				["messages"] = messages
			};

			if (model != null)
				requestBody["model"] = model;

			if (temperature.HasValue)
				requestBody["temperature"] = temperature.Value;

			if (maxTokens.HasValue)
				requestBody["max_tokens"] = maxTokens.Value;

			if (context != null)
				requestBody["context"] = context;

			var content = new StringContent(
				JsonConvert.SerializeObject(requestBody, new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				}),
				Encoding.UTF8,
				"application/json"
			);

			var response = await _httpClient.PostAsync("/v1/chat/completions", content, cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Chat completion failed: {response.StatusCode}",
					responseString,
					(int)response.StatusCode,
					"/v1/chat/completions"
				);
			}

			return JsonConvert.DeserializeObject<OpperChatResponse>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// Create a chat completion with streaming response.
		/// </summary>
		public async IAsyncEnumerable<OpperChatStreamChunk> CompletionsStreamAsync(
			List<OpperMessage> messages,
			string? model = null,
			double? temperature = null,
			int? maxTokens = null,
			[EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			if (messages == null || messages.Count == 0)
				throw new ArgumentException("Messages list cannot be null or empty", nameof(messages));

			var requestBody = new Dictionary<string, object>
			{
				["messages"] = messages,
				["stream"] = true
			};

			if (model != null)
				requestBody["model"] = model;

			if (temperature.HasValue)
				requestBody["temperature"] = temperature.Value;

			if (maxTokens.HasValue)
				requestBody["max_tokens"] = maxTokens.Value;

			var content = new StringContent(
				JsonConvert.SerializeObject(requestBody, new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				}),
				Encoding.UTF8,
				"application/json"
			);

			var request = new HttpRequestMessage(HttpMethod.Post, "/v1/chat/completions")
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
					$"Chat stream failed: {response.StatusCode}",
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

				OpperChatStreamChunk? chunk;
				try
				{
					chunk = JsonConvert.DeserializeObject<OpperChatStreamChunk>(jsonData);
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
		/// Simple helper to get a single completion from a prompt.
		/// </summary>
		public async Task<string> CompleteAsync(
			string prompt,
			string? systemPrompt = null,
			string? model = null,
			double? temperature = null,
			CancellationToken cancellationToken = default)
		{
			var messages = new List<OpperMessage>();

			if (!string.IsNullOrEmpty(systemPrompt))
				messages.Add(OpperMessage.System(systemPrompt));

			messages.Add(OpperMessage.User(prompt));

			var response = await CompletionsAsync(messages, model, temperature, cancellationToken: cancellationToken);

			return response.Content ?? string.Empty;
		}
	}
}
