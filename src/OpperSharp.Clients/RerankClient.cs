using Newtonsoft.Json;
using OpperSharp.Exceptions;
using OpperSharp.Models.Rerank;
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
	/// Client for Opper AI reranking operations (v2 API).
	/// </summary>
	public class RerankClient
	{
		private readonly HttpClient _httpClient;
		private readonly string _rerankEndpoint;

		public RerankClient(HttpClient httpClient)
		{
			_httpClient = httpClient;
			_rerankEndpoint = EndPoints.Rerank.GetDescription();
		}

		/// <summary>
		/// Rerank documents based on query relevance.
		/// </summary>
		public async Task<OpperRerankResponse> RerankAsync(
			string query,
			List<object> documents,
			string model,
			int? topK = null,
			bool? returnDocuments = null,
			CancellationToken cancellationToken = default)
		{
			var request = new OpperRerankRequest
			{
				Query = query,
				Documents = documents,
				Model = model,
				TopK = topK,
				ReturnDocuments = returnDocuments
			};

			var content = new StringContent(
				JsonConvert.SerializeObject(request, new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				}),
				Encoding.UTF8,
				"application/json"
			);

			var response = await _httpClient.PostAsync(_rerankEndpoint, content, cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Reranking failed: {response.StatusCode}",
					responseString,
					(int)response.StatusCode,
					_rerankEndpoint
				);
			}

			return JsonConvert.DeserializeObject<OpperRerankResponse>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// List available reranking models.
		/// </summary>
		public async Task<List<string>> ListModelsAsync(CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.GetAsync($"{_rerankEndpoint}/models", cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to list rerank models: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			return JsonConvert.DeserializeObject<List<string>>(responseString)
				?? new List<string>();
		}
	}
}
