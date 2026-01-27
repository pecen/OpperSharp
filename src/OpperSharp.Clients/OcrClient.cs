using Newtonsoft.Json;
using OpperSharp.Exceptions;
using OpperSharp.Models.Common;
using OpperSharp.Models.Ocr;
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
	/// Client for Opper AI OCR operations (v2 API).
	/// </summary>
	public class OcrClient
	{
		private readonly HttpClient _httpClient;
		private readonly string _ocrEndpoint;

		public OcrClient(HttpClient httpClient)
		{
			_httpClient = httpClient;
			_ocrEndpoint = EndPoints.Ocr.GetDescription();
		}

		/// <summary>
		/// Process OCR on a document.
		/// </summary>
		public async Task<OpperOcrResponse> ProcessAsync(
			OpperOcrRequest request,
			CancellationToken cancellationToken = default)
		{
			var content = new StringContent(
				JsonConvert.SerializeObject(request, new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				}),
				Encoding.UTF8,
				"application/json"
			);

			var response = await _httpClient.PostAsync(_ocrEndpoint, content, cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"OCR processing failed: {response.StatusCode}",
					responseString,
					(int)response.StatusCode,
					_ocrEndpoint
				);
			}

			return JsonConvert.DeserializeObject<OpperOcrResponse>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// List available OCR models.
		/// </summary>
		public async Task<List<string>> ListModelsAsync(CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.GetAsync($"{_ocrEndpoint}/models", cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to list OCR models: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			return JsonConvert.DeserializeObject<List<string>>(responseString)
				?? new List<string>();
		}
	}
}
