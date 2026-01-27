using System.ComponentModel;

namespace OpperSharp.Utilities.Enums
{
	public enum EndPoints
	{
		[Description("/chat/completions")]
		ChatCompletions,
		[Description("/functions")]
		Functions,
		[Description("/call")]
		Calls,
		[Description("/indexes")]
		Indexes,
		[Description("/spans")]
		Spans,
		[Description("/traces")]
		Traces
	}
}
