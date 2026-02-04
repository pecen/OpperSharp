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
		[Description("/knowledge")]
		Knowledge,
		[Description("/datasets")]
		Datasets,
		[Description("/spans")]
		Spans,
		[Description("/traces")]
		Traces,
		[Description("/embeddings")]
		Embeddings,
		[Description("/models")]
		Models,
		[Description("/ocr")]
		Ocr,
		[Description("/rerank")]
		Rerank,
		[Description("/analytics/usage")]
		AnalyticsUsage,
		[Description("/indexes")]
		Indexes
	}
}
