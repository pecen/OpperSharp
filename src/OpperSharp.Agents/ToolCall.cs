using System;
using System.Collections.Generic;

namespace OpperSharp.Agents
{
	/// <summary>
	/// Record of a tool call made by the agent.
	/// </summary>
	public class ToolCall
	{
		/// <summary>
		/// Name of the tool that was called.
		/// </summary>
		public string ToolName { get; set; } = string.Empty;

		/// <summary>
		/// Arguments passed to the tool.
		/// </summary>
		public Dictionary<string, object?> Arguments { get; set; } = new();

		/// <summary>
		/// Result returned by the tool.
		/// </summary>
		public object? Result { get; set; }

		/// <summary>
		/// When the tool was called.
		/// </summary>
		public DateTime Timestamp { get; set; } = DateTime.UtcNow;

		/// <summary>
		/// Duration of the tool call in milliseconds.
		/// </summary>
		public long DurationMs { get; set; }

		/// <summary>
		/// Error if the tool call failed.
		/// </summary>
		public string? Error { get; set; }
	}
}
