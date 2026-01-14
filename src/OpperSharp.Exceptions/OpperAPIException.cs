using System;

namespace OpperSharp.Exceptions
{
	/// <summary>
	/// Exception thrown when the Opper API returns an error.
	/// </summary>
	public class OpperAPIException : Exception
	{
		/// <summary>
		/// The raw response content from the API.
		/// </summary>
		public string ResponseContent { get; }

		/// <summary>
		/// The HTTP status code returned by the API.
		/// </summary>
		public int? StatusCode { get; }

		/// <summary>
		/// The request path that caused the error.
		/// </summary>
		public string? RequestPath { get; }

		public OpperAPIException(
			string message,
			string responseContent,
			int? statusCode = null,
			string? requestPath = null)
			: base(message)
		{
			ResponseContent = responseContent;
			StatusCode = statusCode;
			RequestPath = requestPath;
		}

		public OpperAPIException(
			string message,
			string responseContent,
			Exception innerException,
			int? statusCode = null)
			: base(message, innerException)
		{
			ResponseContent = responseContent;
			StatusCode = statusCode;
		}

		public override string ToString()
		{
			return $"{Message}\nStatus Code: {StatusCode}\nPath: {RequestPath}\nResponse: {ResponseContent}";
		}
	}
}

