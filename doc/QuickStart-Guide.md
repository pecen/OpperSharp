# OpperSharp Quick Start Guide

Get started with OpperSharp in minutes with these practical examples.

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
using OpperSharp.Models.Chat;
using OpperSharp.Models.Functions;
using OpperSharp.Models.Indexes;

// Set your API key as environment variable
// OPPER_API_KEY=your-key-here

var client = OpperClient.FromEnvironment();
// or
var client = new OpperClient("your-api-key");
```

---

## 5-Minute Examples

### Example 1: Simple Chat Completion

```csharp
using OpperSharp.Core;
using OpperSharp.Models.Chat;

var client = OpperClient.FromEnvironment();

// Quick one-liner
var answer = await client.Chat.CompleteAsync(
    prompt: "Explain quantum computing in simple terms",
    systemPrompt: "You are a helpful science teacher"
);

Console.WriteLine(answer);

// Full control version
var response = await client.Chat.CompletionsAsync(
    messages: new List<OpperMessage>
    {
        OpperMessage.System("You are a helpful science teacher"),
        OpperMessage.User("Explain quantum computing in simple terms")
    },
    model: "gpt-4",
    temperature: 0.7,
    maxTokens: 500
);

Console.WriteLine($"Response: {response.Content}");
Console.WriteLine($"Tokens: {response.Usage?.TotalTokens}");
```

---

### Example 2: Build a Knowledge Base

```csharp
using OpperSharp.Core;
using OpperSharp.Models.Indexes;

var client = OpperClient.FromEnvironment();

// Create or get index
var index = await client.Indexes.GetOrCreateAsync(
    name: "company-docs",
    description: "Internal company documentation"
);

// Add documents
var documents = new List<OpperDocument>
{
    new()
    {
        Content = "Our vacation policy allows 20 days per year",
        Metadata = new() { ["category"] = "HR", ["type"] = "policy" }
    },
    new()
    {
        Content = "Code review process requires 2 approvals",
        Metadata = new() { ["category"] = "Engineering", ["type"] = "process" }
    },
    new()
    {
        Content = "Monthly all-hands meetings are on first Friday",
        Metadata = new() { ["category"] = "General", ["type"] = "schedule" }
    }
};

await client.Indexes.AddBulkAsync("company-docs", documents);

// Search the knowledge base
var results = await client.Indexes.QueryAsync(
    indexName: "company-docs",
    query: "How many vacation days do I get?",
    k: 3
);

foreach (var result in results.Results)
{
    Console.WriteLine($"[Score: {result.Score:F2}] {result.Content}");
}
```

**Output:**
```
[Score: 0.92] Our vacation policy allows 20 days per year
[Score: 0.34] Monthly all-hands meetings are on first Friday
[Score: 0.21] Code review process requires 2 approvals
```

---

### Example 3: Streaming Response

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

### Example 4: Function with Retry Logic

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

### Example 5: RAG (Retrieval-Augmented Generation)

```csharp
using OpperSharp.Core;
using OpperSharp.Models.Chat;

var client = OpperClient.FromEnvironment();

// Step 1: Query knowledge base
var searchResults = await client.Indexes.QueryAsync(
    indexName: "company-docs",
    query: "What is our code review process?",
    k: 3
);

// Step 2: Build context from search results
var context = string.Join("\n\n",
    searchResults.Results.Select(r => r.Content)
);

// Step 3: Generate answer with context
var answer = await client.Chat.CompleteAsync(
    prompt: "What is our code review process?",
    systemPrompt: $@"Answer the question using only the following context:

{context}

If the answer is not in the context, say 'I don't have that information.'"
);

Console.WriteLine(answer);
```

---

### Example 6: Traced Operations

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

### Example 7: Agent with Tools

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

### Example 8: Custom Tool with Async Operation

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

### Example 9: Complete Workflow with Error Handling

```csharp
using OpperSharp.Core;
using OpperSharp.Exceptions;
using OpperSharp.Models.Functions;

var client = OpperClient.FromEnvironment();

async Task<string> ProcessCustomerQuery(string query)
{
    try
    {
        // Step 1: Search knowledge base
        var searchResults = await client.Indexes.QueryAsync(
            indexName: "help-docs",
            query: query,
            k: 5
        );

        if (searchResults.Results.Count == 0)
        {
            return "I couldn't find relevant information. Please contact support.";
        }

        // Step 2: Generate response with context
        var context = string.Join("\n",
            searchResults.Results.Select(r => r.Content)
        );

        var response = await client.CallAsync(
            path: "customer-support",
            input: new Dictionary<string, object>
            {
                ["query"] = query,
                ["context"] = context
            },
            options: new OpperCallOptions
            {
                Model = "gpt-4",
                Temperature = 0.3,
                Cache = true
            }
        );

        return response.Message ?? "I'm sorry, I couldn't generate a response.";
    }
    catch (OpperAPIException ex) when (ex.StatusCode == 404)
    {
        Console.WriteLine("Index not found. Creating help-docs index...");
        await client.Indexes.CreateAsync("help-docs");
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

### Example 10: Function Management

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

### 5. Use GetOrCreate patterns

```csharp
// Idempotent index creation
var index = await client.Indexes.GetOrCreateAsync(
    "my-index",
    "Description of index"
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

### Pattern: Conversational Chat

```csharp
var messages = new List<OpperMessage>
{
    OpperMessage.System("You are a helpful assistant")
};

while (true)
{
    Console.Write("You: ");
    var input = Console.ReadLine();
    if (string.IsNullOrEmpty(input)) break;

    messages.Add(OpperMessage.User(input));

    var response = await client.Chat.CompletionsAsync(messages);

    messages.Add(OpperMessage.Assistant(response.Content ?? ""));
    Console.WriteLine($"AI: {response.Content}");
}
```

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

### Pattern: Multi-Index Search

```csharp
var query = "machine learning algorithms";

var searchTasks = new[]
{
    client.Indexes.QueryAsync("technical-docs", query, k: 5),
    client.Indexes.QueryAsync("research-papers", query, k: 5),
    client.Indexes.QueryAsync("blog-posts", query, k: 5)
};

var allResults = await Task.WhenAll(searchTasks);

var combinedResults = allResults
    .SelectMany(r => r.Results)
    .OrderByDescending(r => r.Score)
    .Take(10);

foreach (var result in combinedResults)
{
    Console.WriteLine($"[{result.Score:F2}] {result.Content}");
}
```

---

## Help & Support

- **Documentation**: [docs/](.)
- **Examples**: [examples/](../examples) *(if you create this folder)*
- **Issues**: Create an issue in your repository
- **Opper API Docs**: https://docs.opper.ai

Happy coding with OpperSharp! 🚀
