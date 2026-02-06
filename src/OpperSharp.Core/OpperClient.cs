using OpperSharp.Utilities;
using OpperSharp.Models.Common;
using OpperSharp.Models.Functions;
using OpperSharp.Clients;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace OpperSharp.Core
{
	/// <summary>
	/// Main client for interacting with the Opper AI API.
	/// </summary>
	public class OpperClient : IDisposable
	{
		private readonly HttpClient _httpClient;
		private readonly bool _ownsHttpClient;
		private readonly RetryHandler? _retryHandler;

		/// <summary>
		/// Default base URL for Opper AI API.
		/// </summary>
		public const string DefaultBaseUrl = "https://api.opper.ai/v2";

		/// <summary>
		/// Creates a new instance of OpperClient with an API key.
		/// </summary>
		public OpperClient(string apiKey, string? baseUrl = null, TimeSpan? timeout = null)
			: this(new OpperClientOptions
			{
				ApiKey = apiKey,
				BaseUrl = baseUrl ?? DefaultBaseUrl,
				Timeout = timeout ?? TimeSpan.FromSeconds(120)
			})
		{
		}

		/// <summary>
		/// Creates a new instance of OpperClient with options.
		/// </summary>
		public OpperClient(OpperClientOptions options)
		{
			if (string.IsNullOrWhiteSpace(options.ApiKey))
				throw new ArgumentException("API key cannot be null or empty", nameof(options));

			_httpClient = new HttpClient
			{
				BaseAddress = new Uri(options.BaseUrl),
				Timeout = options.Timeout
			};

			_httpClient.DefaultRequestHeaders.Authorization =
				new AuthenticationHeaderValue("Bearer", options.ApiKey);
			_httpClient.DefaultRequestHeaders.Accept.Add(
				new MediaTypeWithQualityHeaderValue("application/json"));

			if (options.CustomHeaders != null)
			{
				foreach (var header in options.CustomHeaders)
				{
					_httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
				}
			}

			_ownsHttpClient = true;

			if (options.EnableRetries)
			{
				_retryHandler = new RetryHandler(options.MaxRetries);
			}

			InitializeClients();
		}

		/// <summary>
		/// Creates a new instance of OpperClient with a pre-configured HttpClient.
		/// </summary>
		public OpperClient(HttpClient httpClient)
		{
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			_ownsHttpClient = false;

			InitializeClients();
		}

		/// <summary>
		/// Creates a client from environment variables.
		/// </summary>
		public static OpperClient FromEnvironment()
		{
			return new OpperClient(OpperClientOptions.FromEnvironment());
		}

		private void InitializeClients()
		{
			Functions = new FunctionsClient(_httpClient);
			Knowledge = new KnowledgeClient(_httpClient);
			Spans = new SpansClient(_httpClient);
			Datasets = new DatasetsClient(_httpClient);
			Embeddings = new EmbeddingsClient(_httpClient);
			Models = new ModelsClient(_httpClient);
			Ocr = new OcrClient(_httpClient);
			Rerank = new RerankClient(_httpClient);
			Analytics = new AnalyticsClient(_httpClient);

			// Keep Indexes for backwards compatibility (deprecated)
			Indexes = new IndexesClient(_httpClient);
		}

		/// <summary>
		/// Client for function operations.
		/// </summary>
		public FunctionsClient Functions { get; private set; } = null!;

		/// <summary>
		/// Client for knowledge base operations (v2 API - file-based RAG).
		/// </summary>
		public KnowledgeClient Knowledge { get; private set; } = null!;

		/// <summary>
		/// Client for index operations (DEPRECATED - use Knowledge for v2 API).
		/// </summary>
		[Obsolete("Use Knowledge property instead. Indexes is deprecated in v2 API.")]
		public IndexesClient Indexes { get; private set; } = null!;

		/// <summary>
		/// Client for span/tracing operations.
		/// </summary>
		public SpansClient Spans { get; private set; } = null!;

		/// <summary>
		/// Client for dataset operations (v2 API).
		/// </summary>
		public DatasetsClient Datasets { get; private set; } = null!;

		/// <summary>
		/// Client for embeddings operations (v2 API).
		/// </summary>
		public EmbeddingsClient Embeddings { get; private set; } = null!;

		/// <summary>
		/// Client for model and alias operations (v2 API).
		/// </summary>
		public ModelsClient Models { get; private set; } = null!;

		/// <summary>
		/// Client for OCR operations (v2 API).
		/// </summary>
		public OcrClient Ocr { get; private set; } = null!;

		/// <summary>
		/// Client for document reranking operations (v2 API).
		/// </summary>
		public RerankClient Rerank { get; private set; } = null!;

		/// <summary>
		/// Client for analytics operations (v2 API).
		/// </summary>
		public AnalyticsClient Analytics { get; private set; } = null!;

		/// <summary>
		/// Shorthand for calling a function with automatic retries.
		/// </summary>
		public async Task<OpperFunctionResponse> CallAsync(
			string path,
			Dictionary<string, object> input,
			OpperCallOptions? options = null,
			CancellationToken cancellationToken = default)
		{
			if (_retryHandler != null)
			{
				return await _retryHandler.ExecuteAsync(
					() => Functions.CallAsync(path, input, options, cancellationToken),
					cancellationToken
				);
			}

			return await Functions.CallAsync(path, input, options, cancellationToken);
		}

		/// <summary>
		/// Shorthand for calling a function with streaming.
		/// </summary>
		public IAsyncEnumerable<OpperStreamChunk> CallStreamAsync(
			string path,
			Dictionary<string, object> input,
			OpperCallOptions? options = null,
			CancellationToken cancellationToken = default)
		{
			return Functions.CallStreamAsync(path, input, options, cancellationToken);
		}

		/// <summary>
		/// Disposes the HttpClient if owned by this instance.
		/// </summary>
		public void Dispose()
		{
			if (_ownsHttpClient)
			{
				_httpClient.Dispose();
			}
			GC.SuppressFinalize(this);
		}
	}
}
