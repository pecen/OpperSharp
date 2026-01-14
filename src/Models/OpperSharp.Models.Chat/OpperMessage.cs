using Newtonsoft.Json;

namespace OpperSharp.Models.Chat
{
	/// <summary>
	/// Represents a chat message.
	/// </summary>
	public class OpperMessage
	{
		[JsonProperty("role")]
		public string Role { get; set; } = "user";

		[JsonProperty("content")]
		public string Content { get; set; } = string.Empty;

		[JsonProperty("name")]
		public string? Name { get; set; }

		/// <summary>
		/// Creates a system message.
		/// </summary>
		public static OpperMessage System(string content) => new() { Role = "system", Content = content };

		/// <summary>
		/// Creates a user message.
		/// </summary>
		public static OpperMessage User(string content) => new() { Role = "user", Content = content };

		/// <summary>
		/// Creates an assistant message.
		/// </summary>
		public static OpperMessage Assistant(string content) => new() { Role = "assistant", Content = content };
	}
}
