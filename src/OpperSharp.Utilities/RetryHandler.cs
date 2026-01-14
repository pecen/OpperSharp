using OpperSharp.Exceptions;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpperSharp.Utilities
{
	/// <summary>
	/// Handles retry logic for transient failures.
	/// </summary>
	public class RetryHandler
	{
		private readonly int _maxRetries;
		private readonly TimeSpan _initialDelay;
		private readonly double _backoffMultiplier;

		public RetryHandler(
			int maxRetries = 3,
			TimeSpan? initialDelay = null,
			double backoffMultiplier = 2.0)
		{
			_maxRetries = maxRetries;
			_initialDelay = initialDelay ?? TimeSpan.FromSeconds(1);
			_backoffMultiplier = backoffMultiplier;
		}

		public async Task<T> ExecuteAsync<T>(
			Func<Task<T>> operation,
			CancellationToken cancellationToken = default)
		{
			var delay = _initialDelay;
			Exception? lastException = null;

			for (int attempt = 0; attempt <= _maxRetries; attempt++)
			{
				try
				{
					cancellationToken.ThrowIfCancellationRequested();
					return await operation();
				}
				catch (OpperAPIException ex) when (IsRetryable(ex) && attempt < _maxRetries)
				{
					lastException = ex;
					await Task.Delay(delay, cancellationToken);
					delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * _backoffMultiplier);
				}
				catch (HttpRequestException ex) when (attempt < _maxRetries)
				{
					lastException = ex;
					await Task.Delay(delay, cancellationToken);
					delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * _backoffMultiplier);
				}
				catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested && attempt < _maxRetries)
				{
					lastException = ex;
					await Task.Delay(delay, cancellationToken);
					delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * _backoffMultiplier);
				}
			}

			throw lastException ?? new InvalidOperationException("Operation failed after retries");
		}

		public async Task ExecuteAsync(
			Func<Task> operation,
			CancellationToken cancellationToken = default)
		{
			await ExecuteAsync(async () =>
			{
				await operation();
				return true;
			}, cancellationToken);
		}

		private static bool IsRetryable(OpperAPIException ex)
		{
			return ex.StatusCode switch
			{
				429 => true,  // Rate limited
				500 => true,  // Internal server error
				502 => true,  // Bad gateway
				503 => true,  // Service unavailable
				504 => true,  // Gateway timeout
				_ => false
			};
		}
	}
}
