using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpperSharp.Models.Functions
{
	/// <summary>
	/// Options for function calls.
	/// </summary>
	public class OpperCallOptions
	{
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
	}
}
