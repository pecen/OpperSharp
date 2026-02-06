# OpperSharp TestConsole

A comprehensive test program for testing all features in the OpperSharp SDK v2 API with focus on the Agent framework. In particular, tests 1-3 and 10-12 are designed to demonstrate and test Agent functionality in various scenarios, including a realistic consultant matching use case.

## Prerequisites

### 1. Set API Key

The program reads the API key from **User Secrets** (recommended when running from Visual Studio) or environment variable `OPPER_API_KEY`:

**User Secrets (when project is opened in Visual Studio):**
1. Right-click on the project → "Manage User Secrets"
2. Add:
   ```json
   {
     "OPPER_API_KEY": "your-api-key-here"
   }
   ```

**Environment Variable (alternative):**

**Windows (PowerShell):**
```powershell
$env:OPPER_API_KEY="your-api-key-here"
```

or open "Environment Variables" in Windows (click Start or use the Windows key, and type 'env', then click on 'Edit environment variables for your account') and add `OPPER_API_KEY` there.

**Linux/Mac:**
```bash
export OPPER_API_KEY="your-api-key-here"
```

### 2. Build and Run

```bash
cd src/UI/OpperSharp.UI.TestConsole
dotnet run
```

## Important: Ad-hoc Function Calls

**OpperSharp now uses ad-hoc function calls** - you do NOT need to create any named functions in Opper Dashboard!

All functions (name, instructions, model) are sent in the request body. This makes the SDK easier to use and more flexible.

## Menu Features

### AGENT TESTS

**1) Agent: Basic Math**
- Demonstrates the Agent framework with math tools
- Agent can use Add, Multiply, Percentage, Divide, Subtract
- Tools are automatically discovered via `[Tool]` attribute
- Tests 4 mathematical problems automatically
- Shows execution trace with tool calls
- Displays real-time progress with iteration number and tools being called

**2) Agent: Custom Tools**
- Demonstrates custom tools created with `AgentTool.Create()`
- `query_database` - Simulated database returning user profiles
- `get_weather` - Simulated weather API
- Shows how to create tools with lambda functions
- Case-insensitive tool name matching
- Real-time progress showing which tools are being invoked

**3) Agent: Multi-step**
- Demonstrates complex problem-solving with multiple steps
- Calculates total salary costs for 3 departments + employer tax
- Requires multiple tool calls in sequence
- Shows execution trace to follow the agent's reasoning
- Demonstrates how the agent breaks down complex problems
- Progress updates show iteration and tools being used

### BASIC API TESTS

**4) Simple Function Call**
- Tests basic ad-hoc function call
- You can ask a question interactively
- Shows token usage and caching
- Uses claude-sonnet-4 model

**5) Streaming Response**
- Generates a short story based on your topic
- NOTE: Streaming doesn't work with ad-hoc calls in current implementation
- Uses standard CallAsync instead
- Shows creative writing with temperature 0.8

**6) Conversational Chat**
- Interactive conversation via ad-hoc function calls
- Builds up conversation history
- Type 'quit' to exit
- Demonstrates stateful multi-turn conversations

### v2 API TESTS

**7) Knowledge Base**
- Creates a knowledge base with unique timestamp name
- Creates and uploads a test file (oppersharp-info.txt)
- Lists uploaded files with size
- Demonstrates file-based RAG function
- Shows correct handling of `original_filename` from API

**8) Embeddings**
- Generates vector embeddings for 3 texts
- Uses azure/text-embedding-3-large model
- Shows dimensions (typically 3072)
- Calculates similarity between first two texts (dot product)
- Demonstrates batch embedding generation

**9) Model Aliases**
- Lists available models
- Creates model alias with fallback chain (Opus → Sonnet → Haiku)
- Lists all aliases in account
- Deletes test aliases automatically (cleanup)
- Demonstrates reliability via automatic fallbacks

### CONSULTANT MATCHING

**10) Match Consultant to Assignment**
- Basic consultant matching with 3 consultants
- Agent with two tools: `get_consultants` and `calculate_match_score`
- Analyzes Swedish assignment (.NET 8 modernization)
- Calculates match score based on skills, experience, rate
- Provides ranked recommendation with reasoning
- **Works with 4 tool calls in 3 iterations**
- Shows real-time progress with agent thoughts

**11) Scale Test: 25 Consultants with Rich CV Data**
- Tests agent performance with realistic data volume
- 25 consultant profiles with:
  - Detailed skills (7+ per consultant)
  - Project history (3 projects per consultant)
  - Certifications
  - Language skills
  - Availability and rates
- Agent analyzes and selects the most promising candidates
- **Optimized: 6 tool calls in 3 iterations** (get data → analyze → score top 5)
- Demonstrates scalability for production environment
- Real-time progress shows iteration, tools, and agent reasoning

**12) Error Handling: Tools with Failures**
- Tests agent resilience when tools fail
- Three unreliable tools with different failure rates:
  - `unreliable_data_fetch` - 30% failure rate
  - `flaky_calculation` - 25% failure rate
  - `slow_service` - 20% timeout rate
- Agent handles exceptions gracefully
- Shows how to build robust agents
- **Works perfectly with 3 tool calls**
- Consistent behavior with Temperature = 0.0
- Real-time progress updates during execution

### DEBUG UTILITIES

**94) Test Ad-Hoc Function Call**
- Direct HTTP POST to /v2/call
- Demonstrates ad-hoc mode (no named function)
- Raw request/response inspection

**95) Get Function Details**
- Retrieves details for a specific function
- Shows UUID, path, name, instructions, model
- Demonstrates both UUID and path-based calls

**96) Delete all 7 Functions**
- Cleanup utility (if you created named functions anyway)
- Deletes: math-solver, research-agent, problem-solver, general-qa, story-generator, chat-assistant, consultant-matcher

**97) Create all 7 Functions via API**
- Creates named functions programmatically
- NOTE: Not necessary for ad-hoc mode!
- Use only if you want to test named function mode

**98) Initialize/Activate all 7 Functions**
- Calls each function once to activate
- NOTE: Not necessary for ad-hoc mode!

**99) List all Functions**
- Lists all functions in your Opper account
- Shows raw JSON response
- Debug utility for inspecting function state

## Architecture: Consultant Matching

### Basic (Menu 10)

```csharp
var agent = new Agent(client, new AgentOptions
{
    Name = "consultant-matcher",
    OnProgress = (update) =>
    {
        WriteLine($"\n[Iteration {update.Iteration}]");
        if (update.ToolCallCount > 0)
        {
            WriteLine($"  💡 Calling {update.ToolCallCount} tool(s): {string.Join(", ", update.ToolNames)}");
        }
        if (!string.IsNullOrWhiteSpace(update.Message))
        {
            WriteLine($"  🤔 Agent: {update.Message.Substring(0, 150)}...");
        }
    },
    Instructions = "You are an AI consultant matching system...",
    MaxIterations = 10,
    Model = "anthropic/claude-opus-4.5"
})
.WithTool(AgentTool.Create(
    name: "get_consultants",
    description: "Retrieves consultant profiles",
    handler: (string _) => {
        return Task.FromResult(JsonSerializer.Serialize(consultants));
    },
    inputDescription: "Any query string"
))
.WithTool(AgentTool.Create(
    name: "calculate_match_score",
    description: "Calculates match score (0-100)",
    handler: (string input) => {
        var obj = JsonSerializer.Deserialize<Dictionary<string, string>>(input);
        var consultantName = obj["consultantName"];
        // Scoring logic...
        return Task.FromResult(score.ToString());
    },
    inputDescription: "JSON: {\"consultantName\": \"Name\", \"requirements\": \"...\"}"
));

var response = await agent.RunAsync("Analyze this assignment...");
```

### Scale Test (Menu 11)

- 25 consultants with IDs (C001-C025)
- Uses `consultantId` instead of `consultantName`
- Agent analyzes and selects top 5-6 candidates to score
- Optimized to avoid unnecessary tool calls

### Key Learnings from Menu 11 Optimization

**What works:**
- XML examples in instructions: `<function_calls><invoke name="tool"><parameter>...</parameter></invoke></function_calls>`
- CRITICAL RULES section warning against hallucination
- WORKFLOW with numbered steps
- JSON input for tools needing multiple parameters
- Give the agent autonomy to choose which consultants to score

**What does NOT work:**
- Trying to force the agent to call specific tools with hard-coded inputs
- Using "Call tool_name with input X" in user query (triggers description mode)
- Overly aggressive "EXECUTE NOW" instructions
- Trying to mimic Menu 12's pattern when the task is fundamentally different

## Real-time Progress Display

All agent tests (Menu 1-3, 10-12) now show real-time progress during execution:

**OnProgress Callback:**
```csharp
OnProgress = (update) =>
{
    WriteLine($"[Iteration {update.Iteration}]");
    if (update.ToolCallCount > 0)
    {
        WriteLine($"💡 Calling {update.ToolCallCount} tool(s): {string.Join(", ", update.ToolNames)}");
    }
    if (!string.IsNullOrWhiteSpace(update.Message))
    {
        WriteLine($"🤔 Agent: {update.Message.Substring(0, 150)}...");
    }
}
```

**Benefits:**
- Users see what the AI is thinking in real-time
- Shows which tools are being called
- Provides interactive feedback during long-running operations
- Helps understand the agent's problem-solving process

## Example: Production Integration

### 1. Integrate with Database

```csharp
.WithTool(AgentTool.Create(
    name: "get_consultants",
    description: "Query CINode API for available consultants",
    handler: async (string query) =>
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", cinodeApiKey);

        var response = await httpClient.GetAsync(
            "https://api.cinode.com/v1/consultants?available=true"
        );
        var consultants = await response.Content.ReadAsStringAsync();
        return consultants;
    },
    inputDescription: "Search query for consultants (e.g., 'available', 'C# developers')"
))
```

### 2. Enhanced Matching Logic

```csharp
handler: async (string input) =>
{
    var obj = JsonSerializer.Deserialize<Dictionary<string, string>>(input);
    var consultantId = obj["consultantId"];
    var requirements = obj["requirements"];

    // Use Embeddings for semantic matching
    var reqEmbedding = await client.Embeddings.CreateAsync(requirements);
    var consultantProfile = await GetConsultantProfile(consultantId);
    var conEmbedding = await client.Embeddings.CreateAsync(consultantProfile);

    // Cosine similarity
    var similarity = CalculateCosineSimilarity(reqEmbedding, conEmbedding);

    // Combine with rule-based scoring
    var score = (similarity * 50) + RuleBasedScore(consultant, requirements);

    return score.ToString();
}
```

### 3. Add More Tools

```csharp
.WithTool(AgentTool.Create(
    name: "check_availability",
    description: "Check consultant's calendar availability",
    handler: async (string consultantId) => { /* ... */ }
))
.WithTool(AgentTool.Create(
    name: "get_previous_assignments",
    description: "Get consultant's assignment history",
    handler: async (string consultantId) => { /* ... */ }
))
.WithTool(AgentTool.Create(
    name: "calculate_travel_distance",
    description: "Calculate distance between consultant and assignment location",
    handler: async (string input) => { /* ... */ }
))
```

### 4. Use Knowledge Base for RAG

```csharp
// Upload all assignment descriptions
var kb = await client.Knowledge.CreateAsync("assignments-kb");
foreach (var assignment in assignments)
{
    await client.Knowledge.UploadFileAsync(kb.Id, assignment.Title, content);
}

// Use in agent
.WithTool(AgentTool.Create(
    name: "search_similar_assignments",
    description: "Find similar past assignments using RAG",
    handler: async (string query) =>
    {
        // RAG query against knowledge base
        var results = await client.Knowledge.QueryAsync(kb.Id, query);
        return JsonSerializer.Serialize(results);
    }
))
```

## Performance Tips

### Menu 11 Optimization Insights

- **6 tool calls is optimal** for 25 consultants (1 get + 5 score)
- Agent does natural pre-filtering based on requirements
- Don't force specific tool call patterns - give the agent autonomy
- XML examples in instructions are critical to avoid hallucination
- JSON input format works better than simple strings for complex data

### Menu 12 Consistency

- **Temperature = 0.0** ensures deterministic behavior
- Without Temperature = 0.0, Menu 12 had only 25% success rate (0 tool calls 3 out of 4 times)
- XML examples in instructions prevent hallucination
- CRITICAL RULES and WORKFLOW sections provide clear guidance

### General Best Practices

1. **MaxIterations**: Set to 10-15 for complex tasks
2. **Model**: Use claude-opus-4.5 for agents (better reasoning)
3. **Instructions**: Include CRITICAL RULES and XML examples
4. **Error Handling**: See Menu 12 for resilient tool design
5. **Tool Descriptions**: Be specific about input/output format
6. **Temperature**: Use 0.0 for consistent, deterministic behavior
7. **OnProgress**: Add callback for real-time user feedback

## Troubleshooting

### "Could not initialize OpperClient"
- Check User Secrets or OPPER_API_KEY environment variable
- Visual Studio: Right-click project → Manage User Secrets
- Verify: `dotnet user-secrets list`

### Agent hallucination (0 tool calls)
- Check that instructions contain XML examples
- Ensure tool inputDescription matches handler signature
- Menu 10 and 12 are working examples to copy from
- For Menu 12: Ensure Temperature = 0.0 is set
- Avoid using "Call tool_name with input X" language in user queries

### "Tool 'xxx' not found"
- Case-insensitive matching is enabled
- Check spelling in WithTool() and tool name in handler
- Use AgentTool.Create() for simple tools

### Performance issues
- Too many consultants? Implement server-side filtering
- Too many iterations? Adjust MaxIterations or instructions
- See Menu 11 for example of efficient large-scale matching

### Inconsistent behavior
- Set Temperature = 0.0 for deterministic behavior
- LLMs are stochastic - without Temperature = 0.0, same input may produce different outputs
- Menu 12 demonstrated this: 0 tool calls 75% of the time until Temperature was set to 0.0

## Next Steps

1. ✅ **Test all features** - Go through menu 1-12 systematically
2. ✅ **Understand Agent patterns** - Menu 10 (basic), 11 (scale), 12 (errors)
3. 🔄 **Customize Menu 11** - Replace hard-coded consultants with CINode API
4. 🔄 **Improve scoring** - Add embeddings for semantic matching
5. 🔄 **Add tools** - Availability, history, distance, etc.
6. 🔄 **Build GUI** - Create user interface for consultant matching

## Resources

- **OpperSharp Documentation**: `../../doc/`
- **Agent Framework**: See Menu 1-3, 10-12 for examples
- **Opper API Docs**: https://docs.opper.ai
- **Claude Models**: https://docs.anthropic.com/claude/docs

---

**Swedish version available**: [README.sv.md](README.sv.md)
