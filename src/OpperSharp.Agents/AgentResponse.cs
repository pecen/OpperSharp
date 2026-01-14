using System.Collections.Generic;

namespace OpperSharp.Agents
{
	/// <summary>
	/// Response from an agent run.
	/// </summary>
	public class AgentResponse
	{
		/// <summary>
		/// The final output from the agent.
		/// </summary>
		public string Output { get; set; } = string.Empty;

		/// <summary>
		/// Whether the agent completed successfully.
		/// </summary>
		public bool Success { get; set; }

		/// <summary>
		/// Error message if the agent failed.
		/// </summary>
		public string? Error { get; set; }

		/// <summary>
		/// Number of iterations the agent took.
		/// </summary>
		public int Iterations { get; set; }

		/// <summary>
		/// History of tool calls made by the agent.
		/// </summary>
		public List<ToolCall> ToolCalls { get; set; } = new();

		/// <summary>
		/// Span ID for tracing.
		/// </summary>
		public string? SpanId { get; set; }

		/// <summary>
		/// Trace ID for tracing.
		/// </summary>
		public string? TraceId { get; set; }
	}
}
