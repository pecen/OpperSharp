# OpperSharp SDK Usage Examples

This document demonstrates how to use OpperSharp compared to calling the Opper REST API directly.

## Table of Contents
- [Setup & Authentication](#setup--authentication)
- [Function Operations](#function-operations)
- [Index Operations](#index-operations)
- [Chat Completions](#chat-completions)
- [Spans & Tracing](#spans--tracing)
- [Agent Framework](#agent-framework)

---

## Setup & Authentication

### ❌ Raw REST API (with HttpClient)
```csharp
using System.Net.Http;
using System.Net.Http.Headers;

var httpClient = new HttpClient
{
    BaseAddress = new Uri("https://api.opper.ai")
};
httpClient.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", apiKey);
httpClient.DefaultRequestHeaders.Add("X-OPPER-API-KEY", apiKey);
httpClient.DefaultRequestHeaders.Accept.Add(
    new MediaTypeWithQualityHeaderValue("application/json"));
```

### ✅ OpperSharp SDK
```csharp
using OpperSharp.Core;

// Option 1: Direct initialization
var client = new OpperClient("your-api-key");

// Option 2: From environment variable OPPER_API_KEY
var client = OpperClient.FromEnvironment();

// Option 3: With full options
var client = new OpperClient(new OpperClientOptions
{
    ApiKey = "your-api-key",
    BaseUrl = "https://api.opper.ai",
    Timeout = TimeSpan.FromSeconds(120),
    EnableRetries = true,
    MaxRetries = 3
});
```

**Benefits**: No manual header setup, automatic retry logic, built-in error handling.

---

## Function Operations

### 1. Call a Function

#### ❌ Raw REST API
```csharp
using Newtonsoft.Json;
using System.Text;

var requestBody = new
{
    input = new Dictionary<string, object>
    {
        ["question"] = "What is the weather in Paris?"
    },
    model = "gpt-4",
    temperature = 0.7
};

var content = new StringContent(
    JsonConvert.SerializeObject(requestBody),
    Encoding.UTF8,
    "application/json"
);

var response = await httpClient.PostAsync(
    "/v2/call/weather-function",
    content
);

if (!response.IsSuccessStatusCode)
{
    var error = await response.Content.ReadAsStringAsync();
    throw new Exception($"API Error: {error}");
}

var responseString = await response.Content.ReadAsStringAsync();
var result = JsonConvert.DeserializeObject<dynamic>(responseString);
var output = result.output;
```

#### ✅ OpperSharp SDK
```csharp
using OpperSharp.Models.Functions;

var response = await client.CallAsync(
    path: "weather-function",
    input: new Dictionary<string, object>
    {
        ["question"] = "What is the weather in Paris?"
    },
    options: new OpperCallOptions
    {
        Model = "gpt-4",
        Temperature = 0.7
    }
);

var output = response.Output;
var message = response.Message;
```

**Benefits**: Type-safe response, automatic error handling, cleaner syntax, no manual JSON serialization.

---

### 2. Stream a Function Call

#### ❌ Raw REST API
```csharp
var requestBody = new
{
    input = new Dictionary<string, object> { ["prompt"] = "Tell me a story" },
    stream = true
};

var content = new StringContent(
    JsonConvert.SerializeObject(requestBody),
    Encoding.UTF8,
    "application/json"
);

var request = new HttpRequestMessage(HttpMethod.Post, "/v2/call/story-generator")
{
    Content = content
};

using var response = await httpClient.SendAsync(
    request,
    HttpCompletionOption.ResponseHeadersRead
);

using var stream = await response.Content.ReadAsStreamAsync();
using var reader = new StreamReader(stream);

while (!reader.EndOfStream)
{
    var line = await reader.ReadLineAsync();
    if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
        continue;

    var jsonData = line.Substring(6);
    if (jsonData == "[DONE]")
        break;

    var chunk = JsonConvert.DeserializeObject<dynamic>(jsonData);
    Console.Write(chunk.delta);
}
```

#### ✅ OpperSharp SDK
```csharp
await foreach (var chunk in client.CallStreamAsync(
    path: "story-generator",
    input: new Dictionary<string, object> { ["prompt"] = "Tell me a story" }
))
{
    Console.Write(chunk.Delta);
}
```

**Benefits**: Built-in SSE parsing, automatic cleanup, async enumerable pattern.

---

### 3. Create a Function

#### ❌ Raw REST API
```csharp
var functionDef = new
{
    path = "summarizer",
    name = "Document Summarizer",
    description = "Summarizes documents",
    instructions = "Summarize the following document concisely",
    model = "gpt-4",
    temperature = 0.3,
    input_schema = new
    {
        type = "object",
        properties = new
        {
            document = new { type = "string" }
        },
        required = new[] { "document" }
    }
};

var content = new StringContent(
    JsonConvert.SerializeObject(functionDef),
    Encoding.UTF8,
    "application/json"
);

var response = await httpClient.PostAsync("/v2/functions", content);
var responseString = await response.Content.ReadAsStringAsync();

if (!response.IsSuccessStatusCode)
{
    throw new Exception($"Failed to create function: {responseString}");
}

var function = JsonConvert.DeserializeObject<dynamic>(responseString);
```

#### ✅ OpperSharp SDK
```csharp
using Newtonsoft.Json.Linq;

var function = await client.Functions.CreateAsync(
    new OpperFunctionDefinition
    {
        Path = "summarizer",
        Name = "Document Summarizer",
        Description = "Summarizes documents",
        Instructions = "Summarize the following document concisely",
        Model = "gpt-4",
        Temperature = 0.3,
        InputSchema = JObject.FromObject(new
        {
            type = "object",
            properties = new
            {
                document = new { type = "string" }
            },
            required = new[] { "document" }
        })
    }
);

Console.WriteLine($"Created function: {function.Id}");
```

**Benefits**: Type-safe models, validation, cleaner error messages.

---

## Index Operations

### 1. Create an Index and Add Documents

#### ❌ Raw REST API
```csharp
// Create index
var indexRequest = new { name = "knowledge-base", description = "Company knowledge" };
var content = new StringContent(
    JsonConvert.SerializeObject(indexRequest),
    Encoding.UTF8,
    "application/json"
);

var response = await httpClient.PostAsync("/v2/indexes", content);
var indexJson = await response.Content.ReadAsStringAsync();
var index = JsonConvert.DeserializeObject<dynamic>(indexJson);

// Add document
var docRequest = new
{
    content = "AI is transforming software development",
    metadata = new Dictionary<string, object>
    {
        ["category"] = "tech",
        ["date"] = "2026-01-14"
    }
};

content = new StringContent(
    JsonConvert.SerializeObject(docRequest),
    Encoding.UTF8,
    "application/json"
);

response = await httpClient.PostAsync(
    "/v2/indexes/knowledge-base/index",
    content
);
```

#### ✅ OpperSharp SDK
```csharp
// Create index
var index = await client.Indexes.CreateAsync(
    name: "knowledge-base",
    description: "Company knowledge"
);

// Add document
var document = await client.Indexes.AddAsync(
    indexName: "knowledge-base",
    content: "AI is transforming software development",
    metadata: new Dictionary<string, object>
    {
        ["category"] = "tech",
        ["date"] = "2026-01-14"
    }
);

Console.WriteLine($"Document ID: {document.Id}");
```

**Benefits**: Two operations instead of manual request construction, automatic error handling.

---

### 2. Bulk Add Documents

#### ❌ Raw REST API
```csharp
var documents = new[]
{
    new
    {
        content = "Document 1 content",
        metadata = new { category = "A" }
    },
    new
    {
        content = "Document 2 content",
        metadata = new { category = "B" }
    }
};

var bulkRequest = new { documents };
var content = new StringContent(
    JsonConvert.SerializeObject(bulkRequest),
    Encoding.UTF8,
    "application/json"
);

var response = await httpClient.PostAsync(
    "/v2/indexes/knowledge-base/index/bulk",
    content
);

var resultJson = await response.Content.ReadAsStringAsync();
var results = JsonConvert.DeserializeObject<dynamic>(resultJson);
```

#### ✅ OpperSharp SDK
```csharp
using OpperSharp.Models.Indexes;

var documents = new List<OpperDocument>
{
    new OpperDocument
    {
        Content = "Document 1 content",
        Metadata = new Dictionary<string, object> { ["category"] = "A" }
    },
    new OpperDocument
    {
        Content = "Document 2 content",
        Metadata = new Dictionary<string, object> { ["category"] = "B" }
    }
};

var results = await client.Indexes.AddBulkAsync(
    indexName: "knowledge-base",
    documents: documents
);

Console.WriteLine($"Added {results.Count} documents");
```

**Benefits**: Type-safe document models, cleaner API.

---

### 3. Query an Index

#### ❌ Raw REST API
```csharp
var queryRequest = new
{
    query = "How is AI changing development?",
    k = 5,
    filters = new Dictionary<string, object>
    {
        ["category"] = "tech"
    }
};

var content = new StringContent(
    JsonConvert.SerializeObject(queryRequest),
    Encoding.UTF8,
    "application/json"
);

var response = await httpClient.PostAsync(
    "/v2/indexes/knowledge-base/query",
    content
);

var resultJson = await response.Content.ReadAsStringAsync();
var searchResults = JsonConvert.DeserializeObject<dynamic>(resultJson);

foreach (var result in searchResults.results)
{
    Console.WriteLine($"Score: {result.score}, Content: {result.content}");
}
```

#### ✅ OpperSharp SDK
```csharp
var searchResults = await client.Indexes.QueryAsync(
    indexName: "knowledge-base",
    query: "How is AI changing development?",
    k: 5,
    filters: new Dictionary<string, object>
    {
        ["category"] = "tech"
    }
);

foreach (var result in searchResults.Results)
{
    Console.WriteLine($"Score: {result.Score}, Content: {result.Content}");
}
```

**Benefits**: Type-safe results with IntelliSense support.

---

## Chat Completions

### ❌ Raw REST API
```csharp
var chatRequest = new
{
    messages = new[]
    {
        new { role = "system", content = "You are a helpful assistant" },
        new { role = "user", content = "What is machine learning?" }
    },
    model = "gpt-4",
    temperature = 0.7,
    max_tokens = 500
};

var content = new StringContent(
    JsonConvert.SerializeObject(chatRequest),
    Encoding.UTF8,
    "application/json"
);

var response = await httpClient.PostAsync("/v2/chat/completions", content);
var resultJson = await response.Content.ReadAsStringAsync();
var chatResponse = JsonConvert.DeserializeObject<dynamic>(resultJson);

var message = chatResponse.choices[0].message.content;
Console.WriteLine(message);
```

### ✅ OpperSharp SDK
```csharp
using OpperSharp.Models.Chat;

var response = await client.Chat.CompletionsAsync(
    messages: new List<OpperMessage>
    {
        OpperMessage.System("You are a helpful assistant"),
        OpperMessage.User("What is machine learning?")
    },
    model: "gpt-4",
    temperature: 0.7,
    maxTokens: 500
);

Console.WriteLine(response.Content);
Console.WriteLine($"Tokens used: {response.Usage?.TotalTokens}");

// Even simpler with helper method:
var answer = await client.Chat.CompleteAsync(
    prompt: "What is machine learning?",
    systemPrompt: "You are a helpful assistant",
    model: "gpt-4"
);
```

**Benefits**: Helper factory methods (System, User, Assistant), convenience methods, type-safe usage tracking.

---

## Spans & Tracing

### 1. Create and Update a Span

#### ❌ Raw REST API
```csharp
// Create span
var spanRequest = new
{
    name = "data-processing",
    input = new Dictionary<string, object>
    {
        ["records"] = 1000
    },
    metadata = new { version = "1.0" }
};

var content = new StringContent(
    JsonConvert.SerializeObject(spanRequest),
    Encoding.UTF8,
    "application/json"
);

var response = await httpClient.PostAsync("/v2/spans", content);
var spanJson = await response.Content.ReadAsStringAsync();
var span = JsonConvert.DeserializeObject<dynamic>(spanJson);
var spanId = (string)span.id;

try
{
    // Do work...
    var result = ProcessData();

    // Update span with success
    var updateRequest = new
    {
        output = new Dictionary<string, object> { ["processed"] = result },
        status = "completed"
    };

    content = new StringContent(
        JsonConvert.SerializeObject(updateRequest),
        Encoding.UTF8,
        "application/json"
    );

    var request = new HttpRequestMessage(
        new HttpMethod("PATCH"),
        $"/v2/spans/{spanId}"
    )
    {
        Content = content
    };

    await httpClient.SendAsync(request);
}
catch (Exception ex)
{
    // Update span with error
    var errorRequest = new
    {
        status = "error",
        error = ex.Message
    };

    content = new StringContent(
        JsonConvert.SerializeObject(errorRequest),
        Encoding.UTF8,
        "application/json"
    );

    var request = new HttpRequestMessage(
        new HttpMethod("PATCH"),
        $"/v2/spans/{spanId}"
    )
    {
        Content = content
    };

    await httpClient.SendAsync(request);
}
```

#### ✅ OpperSharp SDK
```csharp
// Automatic span management with TraceAsync
var result = await client.Spans.TraceAsync(
    name: "data-processing",
    action: async (span) =>
    {
        return ProcessData();
    },
    input: new Dictionary<string, object>
    {
        ["records"] = 1000
    },
    metadata: new Dictionary<string, object>
    {
        ["version"] = "1.0"
    }
);

// Manual span management (if needed)
var span = await client.Spans.CreateAsync(
    name: "manual-process",
    input: new Dictionary<string, object> { ["task"] = "analyze" }
);

try
{
    var data = await ProcessAsync();

    await client.Spans.UpdateAsync(
        span.Id,
        output: new Dictionary<string, object> { ["result"] = data },
        status: "completed"
    );
}
catch (Exception ex)
{
    await client.Spans.UpdateAsync(
        span.Id,
        status: "error",
        error: ex.Message
    );
}
```

**Benefits**: Automatic error handling, no manual PATCH requests, cleaner exception handling.

---

### 2. Save Feedback

#### ❌ Raw REST API
```csharp
var feedbackRequest = new
{
    score = 5,
    comment = "Excellent results"
};

var content = new StringContent(
    JsonConvert.SerializeObject(feedbackRequest),
    Encoding.UTF8,
    "application/json"
);

await httpClient.PostAsync(
    $"/v2/spans/{spanId}/feedback",
    content
);
```

#### ✅ OpperSharp SDK
```csharp
await client.Spans.SaveFeedbackAsync(
    spanId: spanId,
    score: 5,
    comment: "Excellent results"
);
```

**Benefits**: Simple method call, no manual JSON construction.

---

## Agent Framework

### ❌ Raw REST API
**Note**: The agent framework with tool calling requires complex orchestration with multiple API calls, tool execution loops, and state management. Here's a simplified version:

```csharp
// This is complex and error-prone!
var iteration = 0;
var maxIterations = 10;
var currentInput = new { query = "What's the weather in Paris and convert to Fahrenheit?" };

while (iteration < maxIterations)
{
    // Call function
    var requestBody = new { input = currentInput };
    var content = new StringContent(
        JsonConvert.SerializeObject(requestBody),
        Encoding.UTF8,
        "application/json"
    );

    var response = await httpClient.PostAsync("/v2/call/agent-function", content);
    var resultJson = await response.Content.ReadAsStringAsync();
    var result = JsonConvert.DeserializeObject<dynamic>(resultJson);

    // Check for tool calls
    if (result.output.tool_calls != null)
    {
        var toolResults = new List<object>();

        foreach (var toolCall in result.output.tool_calls)
        {
            // Execute tool manually
            var toolName = (string)toolCall.name;
            var toolArgs = toolCall.arguments;

            object toolResult;
            if (toolName == "get_weather")
            {
                toolResult = GetWeather((string)toolArgs.city);
            }
            else if (toolName == "convert_temperature")
            {
                toolResult = ConvertTemperature(
                    (double)toolArgs.celsius,
                    (string)toolArgs.to_unit
                );
            }
            else
            {
                throw new Exception($"Unknown tool: {toolName}");
            }

            toolResults.Add(new { tool_name = toolName, result = toolResult });
        }

        currentInput = new { tool_results = toolResults };
    }
    else
    {
        // Done!
        Console.WriteLine(result.output.output);
        break;
    }

    iteration++;
}
```

### ✅ OpperSharp SDK
```csharp
using OpperSharp.Agents;

// Define tools with attributes
public class WeatherTools
{
    [Tool("Gets the current weather for a city")]
    public string GetWeather(string city)
    {
        return $"The weather in {city} is 20°C and sunny";
    }

    [Tool("Converts temperature between units")]
    public double ConvertTemperature(double celsius, string toUnit)
    {
        if (toUnit.ToLower() == "fahrenheit")
            return (celsius * 9/5) + 32;
        return celsius;
    }
}

// Create and run agent
var agent = new Agent(client, new AgentOptions
{
    FunctionPath = "weather-agent",
    MaxIterations = 10,
    EnableTracing = true
}).WithTools(new WeatherTools());

var response = await agent.RunAsync(
    "What's the weather in Paris and convert to Fahrenheit?"
);

Console.WriteLine(response.Output);
Console.WriteLine($"Success: {response.Success}");
Console.WriteLine($"Iterations: {response.Iterations}");
Console.WriteLine($"Tool calls: {response.ToolCalls.Count}");

// Or create tools manually
var manualAgent = new Agent(client, new AgentOptions
{
    FunctionPath = "custom-agent"
})
.WithTool(AgentTool.Create(
    name: "search",
    description: "Searches the web",
    handler: async (query) => {
        return await SearchWebAsync(query);
    }
))
.WithTool(AgentTool.Create<WeatherRequest, WeatherResponse>(
    name: "weather",
    description: "Gets weather data",
    handler: async (request) => {
        return await GetWeatherDataAsync(request.City);
    }
));
```

**Benefits**:
- Automatic tool discovery with `[Tool]` attribute
- Built-in iteration loop with max iterations
- Automatic tracing and error handling
- Type-safe tool definitions
- Fluent API for tool registration
- No manual tool execution routing

---

## Summary: OpperSharp Advantages

| Feature | Raw REST API | OpperSharp SDK |
|---------|-------------|----------------|
| **Setup** | ~15 lines | ~1-3 lines |
| **Type Safety** | ❌ Dynamic/untyped | ✅ Full IntelliSense |
| **Error Handling** | Manual checking | Automatic with typed exceptions |
| **Retry Logic** | Manual implementation | Built-in with exponential backoff |
| **Streaming** | Manual SSE parsing | Async enumerable |
| **JSON Handling** | Manual serialization | Automatic |
| **Agent Framework** | ~100+ lines | ~10-20 lines |
| **Code Readability** | Low | High |
| **Maintenance** | High effort | Low effort |

## Conclusion

OpperSharp provides:
- **90% less code** for common operations
- **Type safety** with compile-time checking
- **Built-in best practices** (retries, error handling, resource cleanup)
- **Idiomatic C#** (async/await, IDisposable, fluent APIs)
- **Developer experience** through IntelliSense and clear APIs
- **Production-ready** features like automatic retries and tracing

The SDK transforms complex HTTP operations into simple, readable, type-safe method calls.
