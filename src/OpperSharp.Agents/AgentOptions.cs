using System;

namespace OpperSharp.Agents
{
	/// <summary>
	/// Attribute to mark a method as an agent tool.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class ToolAttribute : Attribute
	{
		/// <summary>
		/// Name of the tool (defaults to method name if not specified).
		/// </summary>
		public string? Name { get; set; }

		/// <summary>
		/// Description of what the tool does (used by the AI to decide when to use it).
		/// </summary>
		public string Description { get; }

		/// <summary>
		/// Creates a new ToolAttribute with a description.
		/// </summary>
		public ToolAttribute(string description)
		{
			Description = description;
		}
	}
}
