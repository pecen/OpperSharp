# OpperSharp Quick Start Guide (v2 API)

Get started with OpperSharp v2 API in minutes with these practical examples.

## Installation

```bash
# Add via NuGet (when published)
dotnet add package OpperSharp

# Or add project reference
dotnet add reference path/to/OpperSharp.Core.csproj
```

## Basic Setup

```csharp
using OpperSharp.Core;
using OpperSharp.Models.Functions;
using OpperSharp.Models.Knowledge;  // v2: Knowledge instead of Indexes

// Set your API key as environment variable
// OPPER_API_KEY=your-key-here

var client = OpperClient.FromEnvironment();
// or
var client = new OpperClient("your-api-key");
```

---

## 5-Minute Examples

### Example 1: Build a Knowledge Base (v2 - File-based)

```csharp
using OpperSharp.Core;
using OpperSharp.Models.Knowledge;

var client = OpperClient.FromEnvironment();

// Create knowledge base
await client.Knowledge.CreateAsync(
    name: "company-docs",
    embeddingModel: "azure/text-embedding-3-large"
);

// Upload PDF document
using var policyFile = File.OpenRead("company-policies.pdf");
await client.Knowledge.UploadFileAsync(
    knowledgeBaseId: "company-docs",
    filename: "company-policies.pdf",
    fileStream: policyFile,
    contentType: "application/pdf"
);

// Upload CSV file
using var scheduleFile = File.OpenRead("team-schedule.csv");
await client.Knowledge.UploadFileAsync(
    knowledgeBaseId: "company-docs",
    filename: "team-schedule.csv",
    fileStream: scheduleFile,
    contentType: "text/csv"
);

// List uploaded files
var files = await client.Knowledge.ListFilesAsync("company-docs");
foreach (var file in files)
{
    Console.WriteLine($"File: {file.Filename} (Size: {file.Size} bytes)");
}

// Use knowledge base with Functions (integrated RAG)
var answer = await client.Functions.CallAsync(
    "answer-from-docs",
    new { question = "How many vacation days do I get?" },
    new OpperCallOptions
    {
        Context = new() { ["knowledge_base"] = "company-docs" }
    }
);

Console.WriteLine($"Answer: {answer.Message}");
```

**v2 Advantage:** Upload entire PDF/CSV files instead of chunking documents manually. Opper handles parsing, chunking, and embedding generation automatically.

---

### Example 2: Streaming Response

```csharp
using OpperSharp.Core;

var client = OpperClient.FromEnvironment();

Console.Write("AI: ");

await foreach (var chunk in client.CallStreamAsync(
    path: "story-generator",
    input: new Dictionary<string, object>
    {
        ["prompt"] = "Write a short story about a robot learning to paint"
    }
))
{
    if (chunk.Delta != null)
    {
        Console.Write(chunk.Delta);
    }
}

Console.WriteLine();
```

---

### Example 3: Function with Retry Logic

```csharp
using OpperSharp.Core;
using OpperSharp.Models.Functions;
using OpperSharp.Utilities;

// Create client with retries enabled
var client = new OpperClient(new OpperClientOptions
{
    ApiKey = Environment.GetEnvironmentVariable("OPPER_API_KEY")!,
    EnableRetries = true,
    MaxRetries = 3,
    Timeout = TimeSpan.FromSeconds(60)
});

try
{
    var response = await client.CallAsync(
        path: "data-analyzer",
        input: new Dictionary<string, object>
        {
            ["data"] = new[] { 1, 2, 3, 4, 5 }
        },
        options: new OpperCallOptions
        {
            Model = "gpt-4",
            Cache = true
        }
    );

    Console.WriteLine($"Result: {response.Message}");
    Console.WriteLine($"Cached: {response.Cached}");
}
catch (OpperAPIException ex)
{
    Console.WriteLine($"Error {ex.StatusCode}: {ex.Message}");
}
```

---

### Example 4: RAG with Knowledge Base (v2 - Integrated)

```csharp
using OpperSharp.Core;

var client = OpperClient.FromEnvironment();

// v2 approach: Direct integration with Functions
var answer = await client.Functions.CallAsync(
    "answer-from-docs",
    new { question = "What is our code review process?" },
    new OpperCallOptions
    {
        Context = new() { ["knowledge_base"] = "company-docs" }
    }
);

Console.WriteLine($"Answer: {answer.Message}");

// Alternative: Manual retrieval if needed
var files = await client.Knowledge.ListFilesAsync("company-docs");
Console.WriteLine($"\nKnowledge base contains {files.Count} files:");
foreach (var file in files)
{
    Console.WriteLine($"  - {file.Filename}");
}
```

**v2 Advantage:** RAG is integrated directly into Functions. Just specify the knowledge_base in Context, and Opper handles retrieval and context injection automatically.

---

### Example 5: Traced Operations

```csharp
using OpperSharp.Core;

var client = OpperClient.FromEnvironment();

// Automatic tracing with error handling
var result = await client.Spans.TraceAsync(
    name: "user-registration",
    action: async (span) =>
    {
        // Your business logic
        var user = await CreateUserAsync("john@example.com");
        await SendWelcomeEmailAsync(user.Email);

        return new
        {
            UserId = user.Id,
            Success = true
        };
    },
    input: new Dictionary<string, object>
    {
        ["email"] = "john@example.com",
        ["source"] = "web"
    },
    metadata: new Dictionary<string, object>
    {
        ["environment"] = "production",
        ["version"] = "1.0"
    }
);

Console.WriteLine($"User registered: {result.UserId}");

// Later, save feedback
await client.Spans.SaveFeedbackAsync(
    spanId: span.Id,
    score: 5,
    comment: "Registration completed successfully"
);
```

---

### Example 6: Agent with Tools

```csharp
using OpperSharp.Core;
using OpperSharp.Agents;

// Define tools
public class MathTools
{
    [Tool("Adds two numbers")]
    public double Add(double a, double b) => a + b;

    [Tool("Multiplies two numbers")]
    public double Multiply(double a, double b) => a * b;

    [Tool("Calculates percentage")]
    public double Percentage(double value, double percent)
    {
        return value * (percent / 100);
    }
}

// Create agent
var client = OpperClient.FromEnvironment();

var agent = new Agent(client, new AgentOptions
{
    FunctionPath = "math-solver",
    MaxIterations = 10,
    EnableTracing = true,
    Model = "gpt-4"
}).WithTools(new MathTools());

// Run agent
var response = await agent.RunAsync(
    "If I have $1000 and invest it with 15% annual return, " +
    "then add $500, what's the total?"
);

Console.WriteLine($"Answer: {response.Output}");
Console.WriteLine($"Iterations: {response.Iterations}");
Console.WriteLine($"Tools used: {response.ToolCalls.Count}");

foreach (var toolCall in response.ToolCalls)
{
    Console.WriteLine($"  - {toolCall.ToolName}({string.Join(", ", toolCall.Arguments.Values)}) = {toolCall.Result}");
}
```

**Output:**
```
Answer: Your total would be $1650
Iterations: 3
Tools used: 3
  - Multiply(1000, 0.15) = 150
  - Add(1000, 150) = 1150
  - Add(1150, 500) = 1650
```

---

### Example 7: Custom Tool with Async Operation

```csharp
using OpperSharp.Core;
using OpperSharp.Agents;
using System.Net.Http;

var client = OpperClient.FromEnvironment();
var httpClient = new HttpClient();

var agent = new Agent(client, new AgentOptions
{
    FunctionPath = "research-agent",
    MaxIterations = 5
})
.WithTool(AgentTool.Create(
    name: "web_search",
    description: "Searches the web for information",
    handler: async (query) =>
    {
        // Call your search API
        var url = $"https://api.example.com/search?q={Uri.EscapeDataString(query)}";
        var response = await httpClient.GetStringAsync(url);
        return response;
    }
))
.WithTool(AgentTool.Create(
    name: "summarize",
    description: "Summarizes long text",
    handler: async (text) =>
    {
        var summary = await client.CallAsync(
            "summarizer",
            new Dictionary<string, object> { ["text"] = text }
        );
        return summary.Message;
    }
));

var result = await agent.RunAsync(
    "Research the latest developments in quantum computing and summarize"
);

Console.WriteLine(result.Output);
```

---

### Example 8: Complete Workflow with Error Handling (v2)

```csharp
using OpperSharp.Core;
using OpperSharp.Exceptions;
using OpperSharp.Models.Functions;

var client = OpperClient.FromEnvironment();

async Task<string> ProcessCustomerQuery(string query)
{
    try
    {
        // Step 1: Use knowledge base with Functions (v2 integrated RAG)
        var response = await client.Functions.CallAsync(
            path: "customer-support",
            input: new { query = query },
            options: new OpperCallOptions
            {
                Model = "reliable-gpt4", // Using model alias with fallback
                Temperature = 0.3,
                Cache = true,
                Context = new() { ["knowledge_base"] = "help-docs" }
            }
        );

        return response.Message ?? "I'm sorry, I couldn't generate a response.";
    }
    catch (OpperAPIException ex) when (ex.StatusCode == 404)
    {
        Console.WriteLine("Knowledge base not found. Creating help-docs...");
        await client.Knowledge.CreateAsync("help-docs");
        return "Please try again.";
    }
    catch (OpperAPIException ex) when (ex.StatusCode == 429)
    {
        Console.WriteLine("Rate limited. Waiting...");
        await Task.Delay(2000);
        return await ProcessCustomerQuery(query); // Retry
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        return "An error occurred. Please try again later.";
    }
}

// Usage
var answer = await ProcessCustomerQuery("How do I reset my password?");
Console.WriteLine(answer);
```

---

### Example 9: Function Management

```csharp
using OpperSharp.Core;
using OpperSharp.Models.Functions;
using Newtonsoft.Json.Linq;

var client = OpperClient.FromEnvironment();

// Create a function
var function = await client.Functions.CreateAsync(new OpperFunctionDefinition
{
    Path = "email-classifier",
    Name = "Email Classifier",
    Description = "Classifies emails as spam, important, or normal",
    Instructions = @"Classify the email as one of: spam, important, normal.
Consider sender, subject, and content.",
    Model = "gpt-4",
    Temperature = 0.1,
    InputSchema = JObject.FromObject(new
    {
        type = "object",
        properties = new
        {
            sender = new { type = "string" },
            subject = new { type = "string" },
            content = new { type = "string" }
        },
        required = new[] { "sender", "subject", "content" }
    }),
    OutputSchema = JObject.FromObject(new
    {
        type = "object",
        properties = new
        {
            category = new { type = "string", @enum = new[] { "spam", "important", "normal" } },
            confidence = new { type = "number" },
            reason = new { type = "string" }
        }
    })
});

Console.WriteLine($"Created function: {function.Id}");

// Use the function
var result = await client.CallAsync(
    path: "email-classifier",
    input: new Dictionary<string, object>
    {
        ["sender"] = "boss@company.com",
        ["subject"] = "Urgent: Q4 Results",
        ["content"] = "Please review the Q4 financial results before the meeting."
    }
);

Console.WriteLine($"Classification: {result.Output}");

// List all functions
var functions = await client.Functions.ListAsync();
foreach (var fn in functions)
{
    Console.WriteLine($"- {fn.Path}: {fn.Description}");
}

// Update function if needed
await client.Functions.UpdateAsync(
    path: "email-classifier",
    definition: new OpperFunctionDefinition
    {
        Temperature = 0.2 // Adjust temperature
    }
);

// Check if exists
if (await client.Functions.ExistsAsync("email-classifier"))
{
    Console.WriteLine("Function exists!");
}
```

---

### Example 10: Generating Embeddings (v2)

```csharp
using OpperSharp.Core;
using OpperSharp.Models.Embeddings;

var client = OpperClient.FromEnvironment();

// Generate single embedding
var embedding = await client.Embeddings.CreateAsync(
    "Machine learning is transforming technology",
    model: "azure/text-embedding-3-large"
);

Console.WriteLine($"Embedding dimensions: {embedding.Count}");
Console.WriteLine($"First 5 values: {string.Join(", ", embedding.Take(5))}");

// Generate batch embeddings
var texts = new[]
{
    "Artificial intelligence is the future",
    "Machine learning powers modern applications",
    "Deep learning enables image recognition"
};

var batchResponse = await client.Embeddings.CreateBatchAsync(
    texts,
    model: "azure/text-embedding-3-large"
);

Console.WriteLine($"\nGenerated {batchResponse.Embeddings.Count} embeddings");
foreach (var (text, idx) in texts.Select((t, i) => (t, i)))
{
    Console.WriteLine($"{idx + 1}. {text} -> {batchResponse.Embeddings[idx].Count} dimensions");
}
```

**Use Case:** Generate embeddings for custom semantic search, similarity matching, or integration with external vector databases.

---

### Example 11: Model Aliases with Automatic Fallback (v2)

```csharp
using OpperSharp.Core;
using OpperSharp.Models.Models;

var client = OpperClient.FromEnvironment();

// Create model alias with fallback chain
var alias = await client.Models.CreateAliasAsync(
    name: "reliable-gpt4",
    fallbackModels: new List<string>
    {
        "gpt-4",           // Primary model
        "gpt-4-turbo",     // Fallback 1
        "gpt-3.5-turbo"    // Fallback 2
    },
    description: "GPT-4 with automatic fallback for high availability"
);

Console.WriteLine($"Created alias: {alias.Name}");
Console.WriteLine($"Fallback chain: {string.Join(" → ", alias.FallbackModels)}");

// Use the alias in function calls
var result = await client.Functions.CallAsync(
    "analyze-sentiment",
    new { text = "This product is amazing!" },
    new OpperCallOptions
    {
        Model = "reliable-gpt4"  // Will use fallback if gpt-4 unavailable
    }
);

Console.WriteLine($"Result: {result.Message}");

// List all aliases
var aliases = await client.Models.ListAliasesAsync();
foreach (var a in aliases)
{
    Console.WriteLine($"\n{a.Name}:");
    foreach (var model in a.FallbackModels)
    {
        Console.WriteLine($"  - {model}");
    }
}

// Delete alias when done
await client.Models.DeleteAliasAsync("reliable-gpt4");
```

**v2 Advantage:** Automatic fallback ensures high availability without manual retry logic.

---

### Example 12: OCR Document Processing (v2)

```csharp
using OpperSharp.Core;
using OpperSharp.Models.Ocr;

var client = OpperClient.FromEnvironment();

// Process scanned document
var documentBytes = await File.ReadAllBytesAsync("invoice.png");

var ocrResult = await client.Ocr.ProcessAsync(new OpperOcrRequest
{
    Model = "gpt-4-vision",
    Document = documentBytes,
    Instructions = "Extract invoice number, date, total amount, and line items. Return as structured JSON."
});

Console.WriteLine("Extracted Text:");
Console.WriteLine(ocrResult.Text);

if (ocrResult.StructuredData != null)
{
    Console.WriteLine("\nStructured Data:");
    Console.WriteLine(ocrResult.StructuredData);
}

// List available OCR models
var ocrModels = await client.Ocr.ListModelsAsync();
Console.WriteLine($"\nAvailable OCR models: {string.Join(", ", ocrModels)}");

// Process with specific extraction
var receiptBytes = await File.ReadAllBytesAsync("receipt.jpg");
var receiptData = await client.Ocr.ProcessAsync(new OpperOcrRequest
{
    Model = "gpt-4-vision",
    Document = receiptBytes,
    Instructions = "Extract: merchant name, date, items purchased, and total amount"
});

Console.WriteLine($"\nReceipt data: {receiptData.Text}");
```

**Use Case:** Extract text from scanned documents, invoices, receipts, handwritten notes, or images.

---

### Example 13: Reranking Search Results (v2)

```csharp
using OpperSharp.Core;
using OpperSharp.Models.Rerank;

var client = OpperClient.FromEnvironment();

// Simulate search results from your system
var searchResults = new List<object>
{
    "Machine learning uses algorithms to learn from data",
    "Python is a popular programming language",
    "Deep learning is a subset of machine learning using neural networks",
    "JavaScript runs in web browsers",
    "Neural networks mimic the human brain structure"
};

// Rerank based on relevance to query
var reranked = await client.Rerank.RerankAsync(
    query: "What is deep learning?",
    documents: searchResults,
    model: "rerank-model",
    topK: 3,  // Return top 3 most relevant
    returnDocuments: true
);

Console.WriteLine("Reranked results:");
foreach (var result in reranked.Results)
{
    Console.WriteLine($"\n[Score: {result.Score:F4}] {result.Document}");
}

// List available rerank models
var rerankModels = await client.Rerank.ListModelsAsync();
Console.WriteLine($"\nAvailable rerank models: {string.Join(", ", rerankModels)}");
```

**v2 Advantage:** Improve RAG quality by reranking retrieved documents based on semantic relevance to the query.

---

### Example 14: Usage Analytics and Cost Tracking (v2)

```csharp
using OpperSharp.Core;
using OpperSharp.Models.Analytics;

var client = OpperClient.FromEnvironment();

// Get last 30 days usage
var usage = await client.Analytics.GetUsageAsync(
    fromDate: DateTime.Now.AddDays(-30),
    toDate: DateTime.Now,
    granularity: "day",
    fields: new List<string> { "tokens", "cost", "requests" },
    groupBy: new List<string> { "function", "model" }
);

Console.WriteLine("Usage Summary:");
var totalCost = 0.0;
var totalRequests = 0;
var totalTokens = 0;

foreach (var entry in usage.Data)
{
    Console.WriteLine($"\nDate: {entry.Date:yyyy-MM-dd}");
    Console.WriteLine($"  Requests: {entry.Requests}");
    Console.WriteLine($"  Tokens: {entry.Tokens:N0}");
    Console.WriteLine($"  Cost: ${entry.Cost:F4}");

    totalRequests += entry.Requests ?? 0;
    totalTokens += entry.Tokens ?? 0;
    totalCost += entry.Cost ?? 0;

    if (entry.GroupedBy != null)
    {
        Console.WriteLine("  By function:");
        foreach (var group in entry.GroupedBy)
        {
            Console.WriteLine($"    - {group.Key}: {group.Value} requests");
        }
    }
}

Console.WriteLine($"\n--- Totals ---");
Console.WriteLine($"Total Requests: {totalRequests:N0}");
Console.WriteLine($"Total Tokens: {totalTokens:N0}");
Console.WriteLine($"Total Cost: ${totalCost:F2}");
Console.WriteLine($"Avg Cost/Request: ${(totalCost / totalRequests):F4}");

// Weekly granularity
var weeklyUsage = await client.Analytics.GetUsageAsync(
    fromDate: DateTime.Now.AddDays(-90),
    toDate: DateTime.Now,
    granularity: "week"
);

Console.WriteLine("\nWeekly trend:");
foreach (var week in weeklyUsage.Data.TakeLast(4))
{
    Console.WriteLine($"Week of {week.Date:MMM dd}: ${week.Cost:F2}");
}
```

**Use Case:** Track API usage, monitor costs, identify expensive operations, and analyze usage patterns.

---

## Best Practices

### 1. Use `using` statement for proper disposal

```csharp
using var client = OpperClient.FromEnvironment();
// client will be disposed automatically
```

### 2. Handle specific exceptions

```csharp
try
{
    var response = await client.CallAsync(...);
}
catch (OpperAPIException ex) when (ex.StatusCode == 404)
{
    // Handle not found
}
catch (OpperAPIException ex) when (ex.StatusCode == 429)
{
    // Handle rate limiting
}
```

### 3. Use CancellationToken for long operations

```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

try
{
    var response = await client.CallAsync(
        path: "long-operation",
        input: data,
        cancellationToken: cts.Token
    );
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation timed out");
}
```

### 4. Enable caching for repeated calls

```csharp
var response = await client.CallAsync(
    path: "expensive-operation",
    input: data,
    options: new OpperCallOptions
    {
        Cache = true // Enable caching
    }
);

Console.WriteLine($"Was cached: {response.Cached}");
```

### 5. Use model aliases for reliability

```csharp
// Create alias with fallback for high availability
await client.Models.CreateAliasAsync(
    "production-model",
    new List<string> { "gpt-4", "gpt-4-turbo", "gpt-3.5-turbo" }
);

// Use in calls - automatic fallback if primary fails
var result = await client.Functions.CallAsync(
    "my-function",
    input,
    new OpperCallOptions { Model = "production-model" }
);
```

### 6. Leverage v2 knowledge bases

```csharp
// Upload entire documents - Opper handles chunking
using var file = File.OpenRead("documentation.pdf");
await client.Knowledge.UploadFileAsync(
    "docs",
    "documentation.pdf",
    file,
    "application/pdf"
);

// Use with integrated RAG
var answer = await client.Functions.CallAsync(
    "qa-bot",
    new { question = query },
    new OpperCallOptions
    {
        Context = new() { ["knowledge_base"] = "docs" }
    }
);
```

---

## Next Steps

1. **Explore the [Full API Reference](SDK-Usage-Examples.md)** for detailed comparisons with REST API
2. **Check out [Advanced Examples](#)** for complex workflows
3. **Read about [Error Handling](#)** best practices
4. **Learn about [Performance Optimization](#)** techniques

---

## Common Patterns

### Pattern: Batch Processing with Progress

```csharp
var items = GetItemsToProcess(); // Returns 1000 items
var processed = 0;

foreach (var batch in items.Chunk(10))
{
    var tasks = batch.Select(async item =>
    {
        var result = await client.CallAsync("processor",
            new Dictionary<string, object> { ["item"] = item }
        );
        Interlocked.Increment(ref processed);
        return result;
    });

    await Task.WhenAll(tasks);
    Console.WriteLine($"Progress: {processed}/{items.Count}");
}
```

### Pattern: Multi-Knowledge Base Search with Reranking (v2)

```csharp
var query = "machine learning algorithms";

// Search multiple knowledge bases (v2 approach)
var kbNames = new[] { "technical-docs", "research-papers", "blog-posts" };

// Collect documents from all knowledge bases
var allDocuments = new List<string>();

foreach (var kb in kbNames)
{
    var files = await client.Knowledge.ListFilesAsync(kb);
    allDocuments.AddRange(files.Select(f => f.Filename));
}

// Use Rerank API to find most relevant (v2)
var reranked = await client.Rerank.RerankAsync(
    query: query,
    documents: allDocuments.Cast<object>().ToList(),
    model: "rerank-model",
    topK: 10
);

Console.WriteLine("Top 10 most relevant documents:");
foreach (var result in reranked.Results)
{
    Console.WriteLine($"[{result.Score:F4}] {result.Document}");
}
```

---

## Help & Support

- **Documentation**: [docs/](.)
- **Examples**: [examples/](../examples) *(if you create this folder)*
- **Issues**: Create an issue in your repository
- **Opper API Docs**: https://docs.opper.ai

Happy coding with OpperSharp! 🚀
