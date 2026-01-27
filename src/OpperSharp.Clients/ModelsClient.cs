using Newtonsoft.Json;
using OpperSharp.Exceptions;
using OpperSharp.Models.Common;
using OpperSharp.Models.Models;
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
	/// Client for Opper AI model and alias operations (v2 API).
	/// </summary>
	public class ModelsClient
	{
		private readonly HttpClient _httpClient;
		private readonly string _modelsEndpoint;

		public ModelsClient(HttpClient httpClient)
		{
			_httpClient = httpClient;
			_modelsEndpoint = EndPoints.Models.GetDescription();
		}

		#region Standard Models

		/// <summary>
		/// List all available models.
		/// </summary>
		public async Task<List<OpperModel>> ListAsync(CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.GetAsync(_modelsEndpoint, cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to list models: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			var result = JsonConvert.DeserializeObject<OpperListResponse<OpperModel>>(responseString);
			return result?.Data ?? new List<OpperModel>();
		}

		#endregion

		#region Custom Models

		/// <summary>
		/// Register a custom model.
		/// </summary>
		public async Task<OpperCustomModel> RegisterCustomModelAsync(
			string name,
			string identifier,
			Dictionary<string, object>? extra = null,
			string? apiKey = null,
			CancellationToken cancellationToken = default)
		{
			var requestBody = new Dictionary<string, object>
			{
				["name"] = name,
				["identifier"] = identifier
			};

			if (extra != null)
				requestBody["extra"] = extra;

			if (apiKey != null)
				requestBody["api_key"] = apiKey;

			var content = new StringContent(
				JsonConvert.SerializeObject(requestBody),
				Encoding.UTF8,
				"application/json"
			);

			var response = await _httpClient.PostAsync($"{_modelsEndpoint}/custom", content, cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to register custom model: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			return JsonConvert.DeserializeObject<OpperCustomModel>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// List custom models.
		/// </summary>
		public async Task<List<OpperCustomModel>> ListCustomModelsAsync(CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.GetAsync($"{_modelsEndpoint}/custom", cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to list custom models: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			var result = JsonConvert.DeserializeObject<OpperListResponse<OpperCustomModel>>(responseString);
			return result?.Data ?? new List<OpperCustomModel>();
		}

		/// <summary>
		/// Get a custom model by ID.
		/// </summary>
		public async Task<OpperCustomModel> GetCustomModelAsync(
			string modelId,
			CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.GetAsync($"{_modelsEndpoint}/custom/{modelId}", cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to get custom model: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			return JsonConvert.DeserializeObject<OpperCustomModel>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// Get a custom model by name.
		/// </summary>
		public async Task<OpperCustomModel> GetCustomModelByNameAsync(
			string name,
			CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.GetAsync($"{_modelsEndpoint}/custom/by-name/{name}", cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to get custom model by name: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			return JsonConvert.DeserializeObject<OpperCustomModel>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// Update a custom model.
		/// </summary>
		public async Task<OpperCustomModel> UpdateCustomModelAsync(
			string modelId,
			string? name = null,
			string? identifier = null,
			Dictionary<string, object>? extra = null,
			string? apiKey = null,
			CancellationToken cancellationToken = default)
		{
			var requestBody = new Dictionary<string, object>();

			if (name != null)
				requestBody["name"] = name;

			if (identifier != null)
				requestBody["identifier"] = identifier;

			if (extra != null)
				requestBody["extra"] = extra;

			if (apiKey != null)
				requestBody["api_key"] = apiKey;

			var content = new StringContent(
				JsonConvert.SerializeObject(requestBody),
				Encoding.UTF8,
				"application/json"
			);

			var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{_modelsEndpoint}/custom/{modelId}")
			{
				Content = content
			};

			var response = await _httpClient.SendAsync(request, cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to update custom model: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			return JsonConvert.DeserializeObject<OpperCustomModel>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// Delete a custom model.
		/// </summary>
		public async Task DeleteCustomModelAsync(
			string modelId,
			CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.DeleteAsync($"{_modelsEndpoint}/custom/{modelId}", cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
				throw new OpperAPIException(
					$"Failed to delete custom model: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}
		}

		#endregion

		#region Model Aliases

		/// <summary>
		/// Create a model alias with fallback support.
		/// </summary>
		public async Task<OpperModelAlias> CreateAliasAsync(
			string name,
			List<string> fallbackModels,
			string? description = null,
			CancellationToken cancellationToken = default)
		{
			var requestBody = new Dictionary<string, object>
			{
				["name"] = name,
				["fallback_models"] = fallbackModels
			};

			if (description != null)
				requestBody["description"] = description;

			var content = new StringContent(
				JsonConvert.SerializeObject(requestBody),
				Encoding.UTF8,
				"application/json"
			);

			var response = await _httpClient.PostAsync($"{_modelsEndpoint}/aliases", content, cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to create model alias: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			return JsonConvert.DeserializeObject<OpperModelAlias>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// List all model aliases.
		/// </summary>
		public async Task<List<OpperModelAlias>> ListAliasesAsync(CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.GetAsync($"{_modelsEndpoint}/aliases", cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to list model aliases: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			var result = JsonConvert.DeserializeObject<OpperListResponse<OpperModelAlias>>(responseString);
			return result?.Data ?? new List<OpperModelAlias>();
		}

		/// <summary>
		/// Get a model alias by ID.
		/// </summary>
		public async Task<OpperModelAlias> GetAliasAsync(
			string aliasId,
			CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.GetAsync($"{_modelsEndpoint}/aliases/{aliasId}", cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to get model alias: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			return JsonConvert.DeserializeObject<OpperModelAlias>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// Get a model alias by name.
		/// </summary>
		public async Task<OpperModelAlias> GetAliasByNameAsync(
			string name,
			CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.GetAsync($"{_modelsEndpoint}/aliases/by-name/{name}", cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to get model alias by name: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			return JsonConvert.DeserializeObject<OpperModelAlias>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// Update a model alias.
		/// </summary>
		public async Task<OpperModelAlias> UpdateAliasAsync(
			string aliasId,
			string? name = null,
			List<string>? fallbackModels = null,
			string? description = null,
			CancellationToken cancellationToken = default)
		{
			var requestBody = new Dictionary<string, object>();

			if (name != null)
				requestBody["name"] = name;

			if (fallbackModels != null)
				requestBody["fallback_models"] = fallbackModels;

			if (description != null)
				requestBody["description"] = description;

			var content = new StringContent(
				JsonConvert.SerializeObject(requestBody),
				Encoding.UTF8,
				"application/json"
			);

			var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{_modelsEndpoint}/aliases/{aliasId}")
			{
				Content = content
			};

			var response = await _httpClient.SendAsync(request, cancellationToken);
			var responseString = await response.Content.ReadAsStringAsync(cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				throw new OpperAPIException(
					$"Failed to update model alias: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}

			return JsonConvert.DeserializeObject<OpperModelAlias>(responseString)
				?? throw new OpperAPIException("Failed to deserialize response", responseString);
		}

		/// <summary>
		/// Delete a model alias.
		/// </summary>
		public async Task DeleteAliasAsync(
			string aliasId,
			CancellationToken cancellationToken = default)
		{
			var response = await _httpClient.DeleteAsync($"{_modelsEndpoint}/aliases/{aliasId}", cancellationToken);

			if (!response.IsSuccessStatusCode)
			{
				var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
				throw new OpperAPIException(
					$"Failed to delete model alias: {response.StatusCode}",
					responseString,
					(int)response.StatusCode
				);
			}
		}

		#endregion
	}
}
