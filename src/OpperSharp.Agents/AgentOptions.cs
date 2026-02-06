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
		/// The path to the function that powers this agent (DEPRECATED - use Name and Instructions instead).
		/// </summary>
		[Obsolete("Named functions are deprecated. Use Name and Instructions instead for ad-hoc calls.")]
		public string? FunctionPath { get; set; }

		/// <summary>
		/// The name for this agent (used in ad-hoc calls).
		/// </summary>
		public string? Name { get; set; }

		/// <summary>
		/// Instructions that define the agent's behavior and capabilities.
		/// </summary>
		public string? Instructions { get; set; }

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

		/// <summary>
		/// Optional callback for progress updates during agent execution.
		/// Called on each iteration with the agent's intermediate thoughts/messages.
		/// </summary>
		public Action<AgentProgressUpdate>? OnProgress { get; set; }
	}

	/// <summary>
	/// Progress update from agent execution.
	/// </summary>
	public class AgentProgressUpdate
	{
		/// <summary>
		/// Current iteration number.
		/// </summary>
		public int Iteration { get; set; }

		/// <summary>
		/// The agent's message/thoughts at this iteration.
		/// </summary>
		public string Message { get; set; } = string.Empty;

		/// <summary>
		/// Number of tool calls detected in this iteration.
		/// </summary>
		public int ToolCallCount { get; set; }

		/// <summary>
		/// Names of tools being called in this iteration.
		/// </summary>
		public List<string> ToolNames { get; set; } = new();
	}
}
