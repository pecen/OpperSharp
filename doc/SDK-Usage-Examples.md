# OpperSharp SDK Usage Examples (v2 API)

This document demonstrates how to use OpperSharp v2 API compared to calling the Opper REST API directly.

## Table of Contents
- [Setup & Authentication](#setup--authentication)
- [Function Operations](#function-operations)
- [Knowledge Operations (v2)](#knowledge-operations-v2)
- [Datasets (v2)](#datasets-v2)
- [Embeddings (v2)](#embeddings-v2)
- [Models & Aliases (v2)](#models--aliases-v2)
- [OCR (v2)](#ocr-v2)
- [Rerank (v2)](#rerank-v2)
- [Analytics (v2)](#analytics-v2)
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

## Knowledge Operations (v2)

> **v2 Change**: The Indexes API has been replaced with Knowledge Bases in v2. Knowledge bases use file uploads (PDF, CSV, TXT) instead of document indexing.

### 1. Create Knowledge Base and Upload File

#### ❌ Raw REST API
```csharp
// Create knowledge base
var kbRequest = new
{
    name = "company-docs",
    embedding_model = "azure/text-embedding-3-large"
};

var content = new StringContent(
    JsonConvert.SerializeObject(kbRequest),
    Encoding.UTF8,
    "application/json"
);

var response = await httpClient.PostAsync("/v2/knowledge", content);
var kbJson = await response.Content.ReadAsStringAsync();
var kb = JsonConvert.DeserializeObject<dynamic>(kbJson);

// Upload file - requires multipart/form-data
var fileContent = new ByteArrayContent(File.ReadAllBytes("documentation.pdf"));
fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

var formContent = new MultipartFormDataContent
{
    { fileContent, "file", "documentation.pdf" }
};

response = await httpClient.PostAsync(
    "/v2/knowledge/company-docs/files",
    formContent
);

var fileJson = await response.Content.ReadAsStringAsync();
var uploadedFile = JsonConvert.DeserializeObject<dynamic>(fileJson);
```

#### ✅ OpperSharp SDK
```csharp
// Create knowledge base
var kb = await client.Knowledge.CreateAsync(
    name: "company-docs",
    embeddingModel: "azure/text-embedding-3-large"
);

// Upload file - SDK handles multipart/form-data
using var fileStream = File.OpenRead("documentation.pdf");
var file = await client.Knowledge.UploadFileAsync(
    knowledgeBaseId: "company-docs",
    filename: "documentation.pdf",
    fileStream: fileStream,
    contentType: "application/pdf"
);

Console.WriteLine($"Uploaded: {file.Filename}, Size: {file.Size} bytes");
```

**Benefits**: Automatic multipart/form-data handling, cleaner file upload API, type-safe responses.

**v2 Advantage**: Upload entire documents instead of chunking manually. Opper handles parsing, chunking, and embedding generation.

---

### 2. List and Manage Files

#### ❌ Raw REST API
```csharp
// List files
var response = await httpClient.GetAsync("/v2/knowledge/company-docs/files");
var filesJson = await response.Content.ReadAsStringAsync();
var files = JsonConvert.DeserializeObject<dynamic>(filesJson);

foreach (var file in files.items)
{
    Console.WriteLine($"{file.filename}: {file.size} bytes");
}

// Delete file
var fileId = "file-id-here";
response = await httpClient.DeleteAsync($"/v2/knowledge/company-docs/files/{fileId}");

if (!response.IsSuccessStatusCode)
{
    throw new Exception("Failed to delete file");
}
```

#### ✅ OpperSharp SDK
```csharp
// List files
var files = await client.Knowledge.ListFilesAsync("company-docs");

foreach (var file in files)
{
    Console.WriteLine($"{file.Filename}: {file.Size} bytes (ID: {file.Id})");
}

// Delete file
await client.Knowledge.DeleteFileAsync(
    knowledgeBaseId: "company-docs",
    fileId: files[0].Id
);

Console.WriteLine("File deleted successfully");
```

**Benefits**: Type-safe file models, automatic pagination handling, cleaner API.

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

### 3. Use Knowledge Base with Functions (Integrated RAG)

#### ❌ Raw REST API
```csharp
// Call function with knowledge base context
var callRequest = new
{
    input = new { question = "How is AI changing development?" },
    model = "gpt-4",
    context = new Dictionary<string, object>
    {
        ["knowledge_base"] = "company-docs"
    }
};

var content = new StringContent(
    JsonConvert.SerializeObject(callRequest),
    Encoding.UTF8,
    "application/json"
);

var response = await httpClient.PostAsync("/v2/call/qa-bot", content);
var resultJson = await response.Content.ReadAsStringAsync();
var result = JsonConvert.DeserializeObject<dynamic>(resultJson);

Console.WriteLine(result.message);
```

#### ✅ OpperSharp SDK
```csharp
var answer = await client.Functions.CallAsync(
    path: "qa-bot",
    input: new { question = "How is AI changing development?" },
    options: new OpperCallOptions
    {
        Model = "gpt-4",
        Context = new() { ["knowledge_base"] = "company-docs" }
    }
);

Console.WriteLine($"Answer: {answer.Message}");
```

**Benefits**: Type-safe context configuration, automatic RAG integration.

**v2 Advantage**: RAG is seamlessly integrated - just specify knowledge_base in context. Opper handles retrieval, chunking, and context injection automatically.

---

## Datasets (v2)

### Create Training Data Entry

#### ❌ Raw REST API
```csharp
var entryRequest = new
{
    input = new { question = "What is machine learning?" },
    output = new { answer = "ML is a subset of AI that learns from data..." },
    expected = new { category = "AI" },
    comment = "Good example for training"
};

var content = new StringContent(
    JsonConvert.SerializeObject(entryRequest),
    Encoding.UTF8,
    "application/json"
);

var response = await httpClient.PostAsync(
    "/v2/datasets/training-data/entries",
    content
);

var entryJson = await response.Content.ReadAsStringAsync();
var entry = JsonConvert.DeserializeObject<dynamic>(entryJson);
```

#### ✅ OpperSharp SDK
```csharp
using OpperSharp.Models.Datasets;

var entry = await client.Datasets.CreateEntryAsync(
    datasetId: "training-data",
    input: new { question = "What is machine learning?" },
    output: new { answer = "ML is a subset of AI that learns from data..." },
    expected: new { category = "AI" },
    comment: "Good example for training"
);

Console.WriteLine($"Entry created: {entry.Id}");
```

**Benefits**: Type-safe entry creation, automatic serialization.

**v2 Use Case**: Collect training data from production for fine-tuning models.

---

## Embeddings (v2)

### Generate Vector Embeddings

#### ❌ Raw REST API
```csharp
var embeddingRequest = new
{
    text = "Machine learning is transforming technology",
    model = "azure/text-embedding-3-large"
};

var content = new StringContent(
    JsonConvert.SerializeObject(embeddingRequest),
    Encoding.UTF8,
    "application/json"
);

var response = await httpClient.PostAsync("/v2/embeddings", content);
var embeddingJson = await response.Content.ReadAsStringAsync();
var result = JsonConvert.DeserializeObject<dynamic>(embeddingJson);

var embedding = result.embedding;
Console.WriteLine($"Dimensions: {embedding.Count}");
```

#### ✅ OpperSharp SDK
```csharp
using OpperSharp.Models.Embeddings;

var embedding = await client.Embeddings.CreateAsync(
    text: "Machine learning is transforming technology",
    model: "azure/text-embedding-3-large"
);

Console.WriteLine($"Dimensions: {embedding.Count}");
Console.WriteLine($"First 5 values: {string.Join(", ", embedding.Take(5))}");

// Batch embeddings
var batchResponse = await client.Embeddings.CreateBatchAsync(
    new[] { "Text 1", "Text 2", "Text 3" },
    model: "azure/text-embedding-3-large"
);

Console.WriteLine($"Generated {batchResponse.Embeddings.Count} embeddings");
```

**Benefits**: Type-safe embedding responses, batch operations support.

**v2 Use Case**: Generate embeddings for custom semantic search or vector database integration.

---

## Models & Aliases (v2)

### Create Model Alias with Fallback

#### ❌ Raw REST API
```csharp
var aliasRequest = new
{
    name = "reliable-gpt4",
    fallback_models = new[] { "gpt-4", "gpt-4-turbo", "gpt-3.5-turbo" },
    description = "GPT-4 with automatic fallback"
};

var content = new StringContent(
    JsonConvert.SerializeObject(aliasRequest),
    Encoding.UTF8,
    "application/json"
);

var response = await httpClient.PostAsync("/v2/models/aliases", content);
var aliasJson = await response.Content.ReadAsStringAsync();
var alias = JsonConvert.DeserializeObject<dynamic>(aliasJson);
```

#### ✅ OpperSharp SDK
```csharp
using OpperSharp.Models.Models;

var alias = await client.Models.CreateAliasAsync(
    name: "reliable-gpt4",
    fallbackModels: new List<string> { "gpt-4", "gpt-4-turbo", "gpt-3.5-turbo" },
    description: "GPT-4 with automatic fallback"
);

Console.WriteLine($"Alias: {alias.Name}");
Console.WriteLine($"Fallback chain: {string.Join(" → ", alias.FallbackModels)}");

// Use alias in function call
var result = await client.Functions.CallAsync(
    "my-function",
    input,
    new OpperCallOptions { Model = "reliable-gpt4" }
);
```

**Benefits**: Type-safe alias management, easy fallback chain configuration.

**v2 Advantage**: Automatic model fallback ensures high availability without manual retry logic.

---

## OCR (v2)

### Process Document with OCR

#### ❌ Raw REST API
```csharp
var documentBytes = File.ReadAllBytes("invoice.png");
var base64Document = Convert.ToBase64String(documentBytes);

var ocrRequest = new
{
    model = "gpt-4-vision",
    document = base64Document,
    instructions = "Extract invoice details"
};

var content = new StringContent(
    JsonConvert.SerializeObject(ocrRequest),
    Encoding.UTF8,
    "application/json"
);

var response = await httpClient.PostAsync("/v2/ocr", content);
var ocrJson = await response.Content.ReadAsStringAsync();
var result = JsonConvert.DeserializeObject<dynamic>(ocrJson);

Console.WriteLine(result.text);
```

#### ✅ OpperSharp SDK
```csharp
using OpperSharp.Models.Ocr;

var documentBytes = await File.ReadAllBytesAsync("invoice.png");

var ocrResult = await client.Ocr.ProcessAsync(new OpperOcrRequest
{
    Model = "gpt-4-vision",
    Document = documentBytes,
    Instructions = "Extract invoice number, date, total amount, and line items as JSON"
});

Console.WriteLine($"Extracted text: {ocrResult.Text}");

if (ocrResult.StructuredData != null)
{
    Console.WriteLine($"Structured data: {ocrResult.StructuredData}");
}
```

**Benefits**: Automatic base64 encoding, type-safe request/response models.

**v2 Use Case**: Extract text from scanned documents, invoices, receipts, or handwritten notes.

---

## Rerank (v2)

### Rerank Search Results

#### ❌ Raw REST API
```csharp
var rerankRequest = new
{
    query = "What is deep learning?",
    documents = new[]
    {
        "Machine learning uses algorithms",
        "Deep learning uses neural networks",
        "Python is a programming language"
    },
    model = "rerank-model",
    top_k = 2
};

var content = new StringContent(
    JsonConvert.SerializeObject(rerankRequest),
    Encoding.UTF8,
    "application/json"
);

var response = await httpClient.PostAsync("/v2/rerank", content);
var rerankJson = await response.Content.ReadAsStringAsync();
var result = JsonConvert.DeserializeObject<dynamic>(rerankJson);

foreach (var item in result.results)
{
    Console.WriteLine($"[{item.score}] {item.document}");
}
```

#### ✅ OpperSharp SDK
```csharp
using OpperSharp.Models.Rerank;

var searchResults = new List<object>
{
    "Machine learning uses algorithms",
    "Deep learning uses neural networks",
    "Python is a programming language"
};

var reranked = await client.Rerank.RerankAsync(
    query: "What is deep learning?",
    documents: searchResults,
    model: "rerank-model",
    topK: 2,
    returnDocuments: true
);

foreach (var result in reranked.Results)
{
    Console.WriteLine($"[Score: {result.Score:F4}] {result.Document}");
}
```

**Benefits**: Type-safe reranking, automatic result sorting.

**v2 Advantage**: Improve RAG quality by reranking retrieved documents based on semantic relevance.

---

## Analytics (v2)

### Query Usage Analytics

#### ❌ Raw REST API
```csharp
var fromDate = DateTime.Now.AddDays(-30).ToString("yyyy-MM-ddTHH:mm:ss");
var toDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");

var url = $"/v2/analytics/usage?from_date={fromDate}&to_date={toDate}&granularity=day";
var response = await httpClient.GetAsync(url);

var analyticsJson = await response.Content.ReadAsStringAsync();
var analytics = JsonConvert.DeserializeObject<dynamic>(analyticsJson);

foreach (var entry in analytics.data)
{
    Console.WriteLine($"{entry.date}: {entry.requests} requests, ${entry.cost}");
}
```

#### ✅ OpperSharp SDK
```csharp
using OpperSharp.Models.Analytics;

var usage = await client.Analytics.GetUsageAsync(
    fromDate: DateTime.Now.AddDays(-30),
    toDate: DateTime.Now,
    granularity: "day",
    fields: new List<string> { "tokens", "cost", "requests" },
    groupBy: new List<string> { "function", "model" }
);

foreach (var entry in usage.Data)
{
    Console.WriteLine($"{entry.Date:yyyy-MM-dd}:");
    Console.WriteLine($"  Requests: {entry.Requests}");
    Console.WriteLine($"  Tokens: {entry.Tokens:N0}");
    Console.WriteLine($"  Cost: ${entry.Cost:F4}");
}
```

**Benefits**: Type-safe analytics queries, automatic date formatting, cleaner grouping/filtering.

**v2 Use Case**: Track API usage, monitor costs, identify expensive operations.

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
