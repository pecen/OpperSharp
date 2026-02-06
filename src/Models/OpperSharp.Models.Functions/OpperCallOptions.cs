using System.Collections.Generic;

namespace OpperSharp.Models.Functions
{
	/// <summary>
	/// Options for function calls.
	/// </summary>
	public class OpperCallOptions
	{
		/// <summary>
		/// Function name for ad-hoc calls (when not using a pre-created function).
		/// </summary>
		public string? Name { get; set; }

		/// <summary>
		/// Instructions for ad-hoc calls (when not using a pre-created function).
		/// </summary>
		public string? Instructions { get; set; }

		/// <summary>
		/// Additional context to pass to the function.
		/// </summary>
		public Dictionary<string, object>? Context { get; set; }

		/// <summary>
		/// Parent span ID for distributed tracing.
		/// </summary>
		public string? ParentSpanId { get; set; }

		/// <summary>
		/// Whether to use cached responses if available.
		/// </summary>
		public bool? Cache { get; set; }

		/// <summary>
		/// Override the model for this specific call.
		/// </summary>
		public string? Model { get; set; }

		/// <summary>
		/// Override temperature for this specific call.
		/// </summary>
		public double? Temperature { get; set; }

		/// <summary>
		/// Custom metadata to attach to this call.
		/// </summary>
		public Dictionary<string, object>? Metadata { get; set; }

		/// <summary>
		/// Tools available for the function to use during this call.
		/// Each tool should have 'name', 'description', and 'parameters' (JSON Schema).
		/// </summary>
		public List<object>? Tools { get; set; }
	}
}
