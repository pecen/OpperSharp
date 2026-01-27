using Newtonsoft.Json;
using OpperSharp.Exceptions;
using OpperSharp.Models.Common;
using OpperSharp.Models.Spans;
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
	/// Client for Opper AI span/tracing operations.
	/// </summary>
	public class SpansClient
	{
		private readonly HttpClient _httpClient;
		private readonly string _spansEndpoint;
		private readonly string _tracesEndpoint;

		public SpansClient(HttpClient httpClient)
		{
			_httpClient = httpClient;

			_spansEndpoint = EndPoints.Spans.GetDescription();
			_tracesEndpoint = EndPoints.Traces.GetDescription();
		}

		/// <summary>
		/// Create a new span.
		/// </summary>
		public async Task<OpperSpan> CreateAsync(
			string name,
			Dictionary<string, object>? input = null,
			string? parentSpanId = null,
			Dictionary<string, object>? metadata = null,
			CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("Span name cannot be null or empty", nameof(name));

			var requestBody = new Dictionary<string, object>
			{
				["name"] = name
			};

			if (input != null)
				requestBody["input"] = input;

			if (parentSpanId != null)
				requestBody["parent_span_id"] = parentSpanId;

			if (metadata != null)
				requestBody["metadata"] = metadata;

			var content = new StringContent(
				JsonConvert.SerializeObject(requestBody),
				Encoding.UTF8,
				"application/json"
			);

			var response = await _httpClient.PostAsync(_spansEndpoint, content, cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to create span: {response.StatusCode}",
					responseString,
					(int)response.StatusCode,
					_spansEndpoint
				);
			}

			return JsonConvert.DeserializeObject<OpperSpan>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// Update an existing span.
		/// </summary>
		public async Task<OpperSpan> UpdateAsync(
			string spanId,
			Dictionary<string, object>? output = null,
			string? status = null,
			string? error = null,
			Dictionary<string, object>? metadata = null,
			CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(spanId))
				throw new ArgumentException("Span ID cannot be null or empty", nameof(spanId));

			var requestBody = new Dictionary<string, object>();

			if (output != null)
				requestBody["output"] = output;

			if (status != null)
				requestBody["status"] = status;

			if (error != null)
				requestBody["error"] = error;

			if (metadata != null)
				requestBody["metadata"] = metadata;

			var content = new StringContent(
				JsonConvert.SerializeObject(requestBody),
				Encoding.UTF8,
				"application/json"
			);

			var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{_spansEndpoint}/{spanId}")
			{
				Content = content
			};

			var response = await _httpClient.SendAsync(request, cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to update span: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			return JsonConvert.DeserializeObject<OpperSpan>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// Get a span by ID.
		/// </summary>
		public async Task<OpperSpan> GetAsync(
			string spanId,
			CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.GetAsync($"{_spansEndpoint}/{spanId}", cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to get span: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			return JsonConvert.DeserializeObject<OpperSpan>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// List spans for a trace.
		/// </summary>
		public async Task<List<OpperSpan>> ListByTraceAsync(
			string traceId,
			CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.GetAsync($"{_tracesEndpoint}/{traceId}/spans", cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to list spans: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			var result = JsonConvert.DeserializeObject<OpperListResponse<OpperSpan>>(responseString);
			return result?.Data ?? new List<OpperSpan>();
		}

		/// <summary>
		/// Save feedback/evaluation for a span.
		/// </summary>
		public async Task SaveFeedbackAsync(
			string spanId,
			int score,
			string? comment = null,
			CancellationToken cancellationToken = default)
		{
			var requestBody = new Dictionary<string, object>
			{
				["score"] = score
			};

			if (comment != null)
				requestBody["comment"] = comment;

			var content = new StringContent(
				JsonConvert.SerializeObject(requestBody),
				Encoding.UTF8,
				"application/json"
			);

			var response = await _httpClient.PostAsync(
				$"{_spansEndpoint}/{spanId}/feedback",
				content,
				cancellationToken
			);

			if (!response.IsSuccessStatusCode)
			{
				var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
				throw new OpperAPIException(
					$"Failed to save feedback: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}
		}

		/// <summary>
		/// Execute an action within a traced span, automatically handling start/end/error.
		/// </summary>
		public async Task<T> TraceAsync<T>(
			string name,
			Func<OpperSpan, Task<T>> action,
			Dictionary<string, object>? input = null,
			string? parentSpanId = null,
			Dictionary<string, object>? metadata = null,
			CancellationToken cancellationToken = default)
		{
			var span = await CreateAsync(name, input, parentSpanId, metadata, cancellationToken);

			try
			{
				var result = await action(span);

				await UpdateAsync(
					span.Id,
					output: new Dictionary<string, object> { ["result"] = result! },
					status: "completed",
					cancellationToken: cancellationToken
				);

				return result;
			}
			catch (Exception ex)
			{
				await UpdateAsync(
					span.Id,
					status: "error",
					error: ex.Message,
					cancellationToken: cancellationToken
				);

				throw;
			}
		}

		/// <summary>
		/// Execute a void action within a traced span.
		/// </summary>
		public async Task TraceAsync(
			string name,
			Func<OpperSpan, Task> action,
			Dictionary<string, object>? input = null,
			string? parentSpanId = null,
			Dictionary<string, object>? metadata = null,
			CancellationToken cancellationToken = default)
		{
			var span = await CreateAsync(name, input, parentSpanId, metadata, cancellationToken);

			try
			{
				await action(span);

				await UpdateAsync(
					span.Id,
					status: "completed",
					cancellationToken: cancellationToken
				);
			}
			catch (Exception ex)
			{
				await UpdateAsync(
					span.Id,
					status: "error",
					error: ex.Message,
					cancellationToken: cancellationToken
				);

				throw;
			}
		}
	}
}
