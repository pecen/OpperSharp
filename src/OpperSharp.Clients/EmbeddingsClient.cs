using Newtonsoft.Json;
using OpperSharp.Exceptions;
using OpperSharp.Models.Embeddings;
using OpperSharp.Utilities.Enums;
using OpperSharp.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpperSharp.Clients
{
	/// <summary>
	/// Client for Opper AI embeddings operations (v2 API).
	/// </summary>
	public class EmbeddingsClient
	{
		private readonly HttpClient _httpClient;
		private readonly string _embeddingsEndpoint;

		public EmbeddingsClient(HttpClient httpClient)
		{
			_httpClient = httpClient;
			_embeddingsEndpoint = _httpClient.BaseAddress + EndPoints.Embeddings.GetDescription();
		}

		/// <summary>
		/// Create embeddings for a single text.
		/// </summary>
		public async Task<List<double>> CreateAsync(
			string text,
			string? model = null,
			CancellationToken cancellationToken = default)
		{
			var response = await CreateBatchAsync(new[] { text }, model, cancellationToken);
			return response.Data.FirstOrDefault()?.Embedding ?? new List<double>();
		}

		/// <summary>
		/// Create embeddings for multiple texts.
		/// </summary>
		public async Task<OpperEmbeddingResponse> CreateBatchAsync(
			IEnumerable<string> texts,
			string? model = null,
			CancellationToken cancellationToken = default)
		{
			var textList = texts.ToList();
			if (textList.Count == 0)
				throw new ArgumentException("Texts list cannot be empty", nameof(texts));

			var requestBody = new OpperEmbeddingRequest
			{
				Input = textList.Count == 1 ? textList[0] : (object)textList,
				Model = model
			};

			var content = new StringContent(
				JsonConvert.SerializeObject(requestBody, new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				}),
				Encoding.UTF8,
				"application/json"
			);

			var response = await _httpClient.PostAsync(_embeddingsEndpoint, content, cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to create embeddings: {response.StatusCode}",
					responseString,
					(int)response.StatusCode,
					_embeddingsEndpoint
				);
			}

			return JsonConvert.DeserializeObject<OpperEmbeddingResponse>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}
	}
}
