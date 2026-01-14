using System;
using System.Collections.Generic;

namespace OpperSharp.Agents
{
	/// <summary>
	/// Configuration options for an AI agent.
	/// </summary>
	public class AgentOptions
	{
		/// <summary>
		/// The path to the function that powers this agent.
		/// </summary>
		public string FunctionPath { get; set; } = string.Empty;

		/// <summary>
		/// Maximum number of iterations the agent can take (default: 10).
		/// </summary>
		public int MaxIterations { get; set; } = 10;

		/// <summary>
		/// Whether to enable tracing/spans for the agent (default: true).
		/// </summary>
		public bool EnableTracing { get; set; } = true;

		/// <summary>
		/// Parent span ID for distributed tracing.
		/// </summary>
		public string? ParentSpanId { get; set; }

		/// <summary>
		/// Additional context to pass to the agent.
		/// </summary>
		public Dictionary<string, object>? Context { get; set; }

		/// <summary>
		/// Custom metadata to attach to agent execution.
		/// </summary>
		public Dictionary<string, object>? Metadata { get; set; }

		/// <summary>
		/// The model to use for this agent (optional override).
		/// </summary>
		public string? Model { get; set; }

		/// <summary>
		/// Temperature for generation (0.0 to 1.0).
		/// </summary>
		public double? Temperature { get; set; }

		/// <summary>
		/// List of tools available to the agent.
		/// </summary>
		public List<AgentTool> Tools { get; set; } = new();
	}
}
