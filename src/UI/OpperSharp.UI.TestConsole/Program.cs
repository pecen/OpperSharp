using OpperSharp.Agents;
using OpperSharp.Core;
using OpperSharp.Models.Embeddings;
using OpperSharp.Models.Functions;
using OpperSharp.Models.Knowledge;
using OpperSharp.Models.Models;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using static System.Console;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using OpperSharp.Exceptions;

namespace OpperSharp.UI.TestConsole
{
	internal class Program
	{
		private static OpperClient? _client;
		private static IConfiguration? _configuration;

		static async Task Main(string[] args)
		{
			try
			{
				// Try User Secrets first, then fall back to Environment Variables
				var userSecretsConfig = new ConfigurationBuilder()
					.AddUserSecrets<Program>()
					.Build();

				var apiKey = userSecretsConfig["OPPER_API_KEY"];
				var apiKeySource = "User Secrets";

				// If User Secrets is empty/null, fall back to Environment Variable
				if (string.IsNullOrWhiteSpace(apiKey))
				{
					apiKey = Environment.GetEnvironmentVariable("OPPER_API_KEY");
					apiKeySource = "Environment Variable";
				}

				if (string.IsNullOrWhiteSpace(apiKey))
				{
					throw new InvalidOperationException(
						"OPPER_API_KEY not found in User Secrets or Environment Variables.\n" +
						"Please set it using 'Manage User Secrets' in Visual Studio or as an environment variable.");
				}

				// Store in configuration for later use (debug method needs it)
				_configuration = new ConfigurationBuilder()
					.AddInMemoryCollection(new Dictionary<string, string?>
					{
						["OPPER_API_KEY"] = apiKey
					})
					.Build();

				// Initialize client
				_client = new OpperClient(apiKey);
				WriteLine($"✓ OpperClient initialized successfully");
				WriteLine($"  Source: {apiKeySource}");
				WriteLine();
			}
			catch (Exception ex)
			{
				WriteLine("ERROR: Could not initialize OpperClient.");
				WriteLine("Make sure OPPER_API_KEY is set in User Secrets or Environment Variables.");
				WriteLine($"Details: {ex.Message}");
				WriteLine("\nPress any key to exit...");
				ReadKey();
				return;
			}

			while (true)
			{
				try
				{
					ShowMenu();
					var input = ReadLine()?.Trim() ?? "";
					WriteLine();

					switch (input)
					{
						// AGENT TESTS (Priority)
						case "1": await TestAgentBasicMath(); break;
						case "2": await TestAgentWithCustomTools(); break;
						case "3": await TestAgentMultiStep(); break;

						// BASIC API TESTS
						case "4": await TestSimpleFunctionCall(); break;
						case "5": await TestStreaming(); break;
						case "6": await TestChatAPI(); break;

						// v2 API TESTS
						case "7": await TestKnowledgeBase(); break;
						case "8": await TestEmbeddings(); break;
						case "9": await TestModelAliases(); break;

						// CONSULTANT MATCHING (End goal)
						case "10": await TestConsultantMatching(); break;
					case "11": await TestScaleConsultantMatching(); break;
					case "12": await TestErrorHandling(); break;

						case "94": await TestAdHocFunctionCall(); break;
						case "95": await DebugGetFunction(); break;
						case "96": await DeleteAllFunctions(); break;
						case "97": await CreateAllFunctions(); break;
						// DEBUG
						case "98": await InitializeFunctions(); break;
						case "99": await DebugListFunctions(); break;
						case "0": WriteLine("Goodbye!"); return;

						default: WriteLine("Invalid option. Try again."); break;
					}
				}
				catch (Exception ex)
				{
					WriteLine();
					WriteLine("═══════════════════════════════════════");
					WriteLine("ERROR:");
					WriteLine("═══════════════════════════════════════");
					var current = ex;
					while (current != null)
					{
						WriteLine($"  {current.Message}");
						current = current.InnerException;
					}
					WriteLine("═══════════════════════════════════════");
				}

				WriteLine();
				WriteLine("Press <ENTER> to return to menu...");
				ReadLine();
			}
		}

		static void ShowMenu()
		{
			Clear();
			WriteLine("╔═══════════════════════════════════════════════════════════════╗");
			WriteLine("║           OpperSharp v2 API Test Console                      ║");
			WriteLine("╚═══════════════════════════════════════════════════════════════╝");
			WriteLine();
			WriteLine("  AGENT TESTS (Priority):");
			WriteLine("  1) Agent: Basic Math (Calculator with tools)");
			WriteLine("  2) Agent: Custom Tools (Database query simulation)");
			WriteLine("  3) Agent: Multi-step (Complex reasoning)");
			WriteLine();
			WriteLine("  BASIC API TESTS:");
			WriteLine("  4) Simple Function Call");
			WriteLine("  5) Streaming Response");
			WriteLine("  6) Conversational Chat (via Functions)");
			WriteLine();
			WriteLine("  v2 API TESTS:");
			WriteLine("  7) Knowledge Base (File-based RAG)");
			WriteLine("  8) Embeddings (Vector generation)");
			WriteLine("  9) Model Aliases (Fallback chains)");
			WriteLine();
			WriteLine("  CONSULTANT MATCHING:");
			WriteLine("  10) Match Consultant to Assignment");
			WriteLine("  11) Scale Test: 25 Consultants with Rich CV Data");
			WriteLine("  12) Error Handling: Tools with Failures");
			WriteLine();
			WriteLine("  DEBUG:");
			WriteLine("  94) Test Ad-Hoc Function Call (inline/unnamed)");
			WriteLine("  95) Get Function Details (math-solver)");
			WriteLine("  96) Delete all 7 Functions (cleanup)");
			WriteLine("  97) Create all 7 Functions via API");
			WriteLine("  98) Initialize/Activate all 7 Functions");
			WriteLine("  99) List all Functions in Opper account");
			WriteLine();
			WriteLine("  0) Exit");
			WriteLine();
			Write("  Select option and press ENTER > ");
		}

		// ═══════════════════════════════════════════════════════════════
		// AGENT TESTS (Priority)
		// ═══════════════════════════════════════════════════════════════

		static async Task TestAgentBasicMath()
		{
			WriteLine("═══════════════════════════════════════");
			WriteLine("TEST: Agent with Math Tools");
			WriteLine("═══════════════════════════════════════");
			WriteLine();
			WriteLine("This agent can perform mathematical calculations using tools.");
			WriteLine();

			// Create agent with math tools using ad-hoc mode
			var agent = new Agent(_client!, new AgentOptions
			{
				Name = "math-solver",
				Instructions = "You are a helpful math solver. You have access to calculation tools (Add, Multiply, Percentage, Divide, Subtract). Use them to solve mathematical problems step by step. Always use the tools instead of calculating manually.",
				MaxIterations = 10,
				EnableTracing = true,
				Model = "anthropic/claude-opus-4.5"
			}).WithTools(new MathTools());

			WriteLine("Agent created with tools: Add, Multiply, Percentage");
			WriteLine();

			var testProblems = new[]
			{
				"What is 125 + 89?",
				"Calculate 15 * 23",
				"What is 20% of 500?",
				"If I have $1000 and invest it with 12% return, then add $300, what's the total?"
			};

			foreach (var problem in testProblems)
			{
				WriteLine($"❓ Question: {problem}");
				Write("   Working");

				var response = await agent.RunAsync(problem);

				Write($"\r   ✓ Answer: {response.Output}");
				WriteLine($" ({response.Iterations} iterations, {response.ToolCalls.Count} tool calls)");

				if (response.ToolCalls.Any())
				{
					WriteLine("   Tools used:");
					foreach (var call in response.ToolCalls)
					{
						WriteLine($"     - {call.ToolName}({string.Join(", ", call.Arguments.Values)}) = {call.Result}");
					}
				}
				WriteLine();
			}
		}

		static async Task TestAgentWithCustomTools()
		{
			WriteLine("═══════════════════════════════════════");
			WriteLine("TEST: Agent with Custom Tools");
			WriteLine("═══════════════════════════════════════");
			WriteLine();
			WriteLine("This agent has access to simulated database and weather tools.");
			WriteLine();

			var agent = new Agent(_client!, new AgentOptions
			{
				Name = "research-agent",
				Instructions = @"You are a research assistant with access to EXACTLY TWO tools:

1. query_database - Returns ALL users in the database with their roles and skills. Pass any search query (e.g., 'developers', 'Python skills', or just 'all users'). Returns JSON array of user objects.

2. get_weather - Gets weather for a specific city. Pass the city name as input.

IMPORTANT: These are the ONLY tools available. Do NOT try to call any other tools like 'get_users', 'search_users', etc. Always use query_database for user-related queries and get_weather for weather queries.",
				MaxIterations = 10,
				Model = "anthropic/claude-opus-4.5"
			})
			.WithTool(AgentTool.Create(
				name: "query_database",
				description: "Queries the user database and returns all users with their roles and skills as JSON. Use this tool for ANY user-related queries (finding developers, searching by skills, listing all users, etc.)",
				handler: (string query) =>
				{
					// Simulate database query - always returns all users
					return Task.FromResult(JsonSerializer.Serialize(new[]
					{
						new { Name = "Anna Andersson", Role = "Senior Developer", Skills = new[] { "C#", ".NET", "Azure" } },
						new { Name = "Erik Eriksson", Role = "Tech Lead", Skills = new[] { "Python", "AI/ML", "AWS" } },
						new { Name = "Maria Svensson", Role = "Architect", Skills = new[] { "Architecture", "Microservices", "Kubernetes" } }
					}));
				},
				inputDescription: "Any search query for users (e.g., 'developers', 'Python skills', 'all users'). The tool returns all users and you should filter the results."
			))
			.WithTool(AgentTool.Create(
				name: "get_weather",
				description: "Gets current weather information for a specific city",
				handler: (string city) =>
				{
					// Simulate weather API
					var temp = new Random().Next(15, 25);
					return Task.FromResult($"Temperature in {city}: {temp}°C, Partly cloudy");
				},
				inputDescription: "The name of the city to get weather for (e.g., 'Stockholm', 'London', 'New York')"
			));

			WriteLine("Agent created with tools: query_database, get_weather");
			WriteLine();

			var queries = new[]
			{
				"Who are the developers in the database?",
				"What's the weather in Stockholm?",
				"Find someone with Python skills"
			};

			foreach (var query in queries)
			{
				WriteLine($"❓ Query: {query}");
				var response = await agent.RunAsync(query);
				WriteLine($"   ✓ Response: {response.Output}");
				if (response.ToolCalls.Any())
				{
					WriteLine($"   Tools used: {string.Join(", ", response.ToolCalls.Select(tc => tc.ToolName))}");
				}
				WriteLine();
			}
		}

		static async Task TestAgentMultiStep()
		{
			WriteLine("═══════════════════════════════════════");
			WriteLine("TEST: Agent Multi-step Reasoning");
			WriteLine("═══════════════════════════════════════");
			WriteLine();
			WriteLine("Testing complex problem that requires multiple steps and tool calls.");
			WriteLine();

			var agent = new Agent(_client!, new AgentOptions
			{
				Name = "problem-solver",
				Instructions = "You are a problem solver that can break down complex problems into steps and use mathematical tools to solve them. Always use the available calculation tools.",
				MaxIterations = 15,
				EnableTracing = true,
				Model = "anthropic/claude-opus-4.5"
			}).WithTools(new MathTools());

			var complexProblem = @"
A company has 3 departments:
- Sales: 25 employees, average salary 45000 kr/month
- Tech: 40 employees, average salary 55000 kr/month
- Admin: 15 employees, average salary 38000 kr/month

Question: What is the total monthly salary cost?
Then calculate what 18% employer tax on that total would be.";

			WriteLine("Problem:");
			WriteLine(complexProblem);
			WriteLine();
			WriteLine("Agent working...");

			var response = await agent.RunAsync(complexProblem);

			WriteLine();
			WriteLine($"✓ Final Answer: {response.Output}");
			WriteLine($"  Iterations: {response.Iterations}");
			WriteLine($"  Tool calls: {response.ToolCalls.Count}");
			WriteLine();
			WriteLine("Execution trace:");
			foreach (var call in response.ToolCalls)
			{
				WriteLine($"  {call.ToolName}({string.Join(", ", call.Arguments.Values)}) → {call.Result}");
			}
		}

		// ═══════════════════════════════════════════════════════════════
		// BASIC API TESTS
		// ═══════════════════════════════════════════════════════════════

		static async Task TestSimpleFunctionCall()
		{
			WriteLine("═══════════════════════════════════════");
			WriteLine("TEST: Simple Function Call");
			WriteLine("═══════════════════════════════════════");
			WriteLine();
			Write("Enter your question: ");
			var question = ReadLine() ?? "What is artificial intelligence?";
			WriteLine();

			WriteLine("Calling Opper function...");

			// Use ad-hoc mode (no named function required)
			var response = await _client!.Functions.CallAsync(
				path: null,  // Ad-hoc mode
				input: new Dictionary<string, object>
				{
					["question"] = question
				},
				options: new OpperCallOptions
				{
					Name = "general-qa",
					Instructions = "You are a helpful assistant that answers general questions clearly and concisely.",
					Model = "anthropic/claude-sonnet-4",
					Temperature = 0.7
				}
			);

			WriteLine();
			WriteLine("Response:");
			WriteLine($"  Message: {response.Message}");
			WriteLine($"  Tokens: {response.Usage?.TotalTokens}");
			WriteLine($"  Cached: {response.Cached}");
		}

		static async Task TestStreaming()
		{
			WriteLine("═══════════════════════════════════════");
			WriteLine("TEST: Streaming Response");
			WriteLine("═══════════════════════════════════════");
			WriteLine();
			Write("Enter a topic for a short story: ");
			var topic = ReadLine() ?? "a robot learning to paint";
			WriteLine();

			WriteLine("Generating story...");
			WriteLine();

			// Use ad-hoc mode (no named function required)
			// Note: Streaming doesn't appear to work with ad-hoc calls, using regular CallAsync instead
			var response = await _client!.Functions.CallAsync(
				path: null,  // Ad-hoc mode
				input: new Dictionary<string, object>
				{
					["topic"] = topic
				},
				options: new OpperCallOptions
				{
					Name = "story-generator",
					Instructions = "You are a creative writer. Generate engaging short stories based on the given topic. Keep the story concise (2-3 paragraphs).",
					Model = "anthropic/claude-sonnet-4.5",
					Temperature = 0.8
				}
			);

			WriteLine("AI: ");
			WriteLine(response.Message);
			WriteLine();
			WriteLine($"Tokens: {response.Usage?.TotalTokens}");
		}

		static async Task TestChatAPI()
		{
			WriteLine("═══════════════════════════════════════");
			WriteLine("TEST: Conversational Chat (via Functions)");
			WriteLine("═══════════════════════════════════════");
			WriteLine();
			WriteLine("Simple conversational chat. Type 'quit' to exit.");
			WriteLine();

			var conversationHistory = new List<string>();

			while (true)
			{
				Write("You: ");
				var input = ReadLine();
				if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "quit")
					break;

				conversationHistory.Add($"User: {input}");

				// Use ad-hoc mode (no named function required)
				var response = await _client!.Functions.CallAsync(
					path: null,  // Ad-hoc mode
					input: new Dictionary<string, object>
					{
						["message"] = input,
						["history"] = string.Join("\n", conversationHistory.Take(conversationHistory.Count - 1))
					},
					options: new OpperCallOptions
					{
						Name = "chat-assistant",
						Instructions = @"You are a helpful AI assistant for conversational chat.

Guidelines:
- Be friendly and concise
- Consider the conversation history when responding
- Ask clarifying questions when needed
- Provide helpful, relevant answers
- Maintain context throughout the conversation

The user will provide:
- ""message"": Their current message
- ""history"": Previous conversation (if any)

Respond naturally and conversationally.",
						Model = "anthropic/claude-sonnet-4.5",
						Temperature = 0.8
					}
				);

				var aiResponse = response.Message ?? "";
				conversationHistory.Add($"Assistant: {aiResponse}");
				WriteLine($"AI: {aiResponse}");
				WriteLine();
			}
		}

		// ═══════════════════════════════════════════════════════════════
		// v2 API TESTS
		// ═══════════════════════════════════════════════════════════════

		static async Task TestKnowledgeBase()
		{
			WriteLine("═══════════════════════════════════════");
			WriteLine("TEST: Knowledge Base (v2 API)");
			WriteLine("═══════════════════════════════════════");
			WriteLine();
			WriteLine("This demonstrates the file-based RAG system in v2 API.");
			WriteLine();

			var kbName = "test-kb-" + DateTime.Now.Ticks;

			WriteLine($"1. Creating knowledge base: {kbName}");
			var kb = await _client!.Knowledge.CreateAsync(
				name: kbName,
				embeddingModel: "azure/text-embedding-3-large"
			);
			WriteLine($"   ✓ Knowledge base created with ID: {kb.Id}");
			WriteLine();

			WriteLine("2. Creating sample document...");
			var sampleDoc = @"
OpperSharp är en C# SDK för Opper AI v2 API.

Viktiga funktioner:
- Functions API för AI-anrop
- Knowledge Bases för filbaserad RAG
- Agent framework med verktyg
- Embeddings för vektorgenerering
- Model aliases för automatisk fallback
- OCR för dokumentprocessering

Version: v2 API
Språk: C#
Framework: .NET 8.0
";

			var tempFile = Path.Combine(Path.GetTempPath(), "oppersharp-info.txt");
			await File.WriteAllTextAsync(tempFile, sampleDoc);
			WriteLine($"   ✓ Created: {tempFile}");
			WriteLine();

			WriteLine("3. Uploading file to knowledge base...");
			using var fileStream = File.OpenRead(tempFile);
			await _client.Knowledge.UploadFileAsync(
				knowledgeBaseId: kb.Id,
				filename: "oppersharp-info.txt",
				fileStream: fileStream,
				contentType: "text/plain"
			);
			WriteLine("   ✓ File uploaded");
			WriteLine();

			WriteLine("4. Listing files...");
			var files = await _client.Knowledge.ListFilesAsync(kb.Id);
			foreach (var file in files)
			{
				WriteLine($"   - {file.Filename} ({file.Size} bytes)");
			}
			WriteLine();

			WriteLine("5. Querying with RAG (integrated with Functions)...");
			WriteLine("   Note: This requires a function named 'qa-bot' in Opper");
			WriteLine("   You'll need to create it manually for this to work.");
			WriteLine();

			// Clean up
			File.Delete(tempFile);
			WriteLine($"✓ Test completed. Knowledge base '{kbName}' created (not deleted - do manually if needed)");
		}

		static async Task TestEmbeddings()
		{
			WriteLine("═══════════════════════════════════════");
			WriteLine("TEST: Embeddings API (v2)");
			WriteLine("═══════════════════════════════════════");
			WriteLine();

			var texts = new[]
			{
				"Machine learning is a subset of artificial intelligence",
				"Neural networks are inspired by biological neurons",
				"Python is a popular programming language"
			};

			WriteLine("Generating embeddings for 3 texts...");
			WriteLine();

			var batchResponse = await _client!.Embeddings.CreateBatchAsync(
				texts,
				model: "azure/text-embedding-3-large"
			);

			WriteLine($"✓ Generated {batchResponse.Data.Count} embeddings");
			WriteLine();

			for (int i = 0; i < texts.Length; i++)
			{
				WriteLine($"{i + 1}. \"{texts[i]}\"");
				WriteLine($"   Vector dimensions: {batchResponse.Data[i].Embedding.Count}");
				WriteLine($"   First 5 values: {string.Join(", ", batchResponse.Data[i].Embedding.Take(5).Select(v => v.ToString("F4")))}");
				WriteLine();
			}

			// Calculate similarity between first two (simple dot product)
			var emb1 = batchResponse.Data[0].Embedding;
			var emb2 = batchResponse.Data[1].Embedding;
			var similarity = emb1.Zip(emb2, (a, b) => a * b).Sum();

			WriteLine($"Similarity between text 1 and 2: {similarity:F4}");
			WriteLine("(Higher = more similar)");
		}

		static async Task TestModelAliases()
		{
			WriteLine("═══════════════════════════════════════");
			WriteLine("TEST: Model Aliases (v2)");
			WriteLine("═══════════════════════════════════════");
			WriteLine();
			WriteLine("Model aliases provide automatic fallback chains for reliability.");
			WriteLine();

			WriteLine("0. Listing available models to use...");
			try
			{
				var availableModels = await _client!.Models.ListAsync();
				WriteLine($"   Found {availableModels.Count} models");
				if (availableModels.Count > 0)
				{
					WriteLine("   First 10 models:");
					foreach (var model in availableModels.Take(10))
					{
						WriteLine($"   - {model.Name ?? model.Id}");
					}
				}
				WriteLine();
			}
			catch (Exception ex)
			{
				WriteLine($"   (Could not list models: {ex.Message})");
				WriteLine();
			}

			var aliasName = "test-reliable-claude-" + DateTime.Now.Ticks;

			WriteLine($"1. Creating model alias: {aliasName}");
			WriteLine("   Using Claude models with fallback chain...");
			var alias = await _client!.Models.CreateAliasAsync(
				name: aliasName,
				fallbackModels: new List<string>
				{
					"anthropic/claude-opus-4.5",
					"anthropic/claude-sonnet-4",
					"anthropic/claude-3.5-haiku"
				},
				description: "Test alias with Claude fallback chain (Opus → Sonnet → Haiku)"
			);

			WriteLine($"   ✓ Alias created: {alias.Name}");
			WriteLine($"   ID: {alias.Id}");
			WriteLine($"   Fallback chain: {string.Join(" → ", alias.FallbackModels)}");
			WriteLine();

			WriteLine("2. Listing all aliases...");
			var aliases = await _client.Models.ListAliasesAsync();
			WriteLine($"   Found {aliases.Count} aliases:");
			foreach (var a in aliases)
			{
				WriteLine($"   - {a.Name}: {string.Join(", ", a.FallbackModels.Take(2))}...");
			}
			WriteLine();

			WriteLine("3. Cleaning up all test aliases...");
			var testAliases = aliases.Where(a => a.Name.StartsWith("test-reliable-")).ToList();
			if (testAliases.Count > 0)
			{
				WriteLine($"   Found {testAliases.Count} test alias(es) to delete");
				foreach (var testAlias in testAliases)
				{
					await _client.Models.DeleteAliasAsync(testAlias.Id);
					WriteLine($"   ✓ Deleted: {testAlias.Name}");
				}
			}
			else
			{
				WriteLine("   No test aliases to clean up");
			}
		}

		// ═══════════════════════════════════════════════════════════════
		// CONSULTANT MATCHING (End Goal)
		// ═══════════════════════════════════════════════════════════════

		static async Task TestConsultantMatching()
		{
			WriteLine("═══════════════════════════════════════");
			WriteLine("CONSULTANT MATCHING SYSTEM");
			WriteLine("═══════════════════════════════════════");
			WriteLine();
			WriteLine("This demonstrates using Agents to match consultants with assignments.");
			WriteLine();

			// Sample consultant profiles
			var consultants = new[]
			{
				new
				{
					Name = "Anna Andersson",
					Title = "Senior .NET Developer",
					Skills = new[] { "C#", ".NET 8", "Azure", "Microservices", "REST API", "SQL Server" },
					Experience = "10 years",
					Availability = "Immediate",
					Rate = "1200 SEK/hour",
					Languages = new[] { "Swedish", "English" }
				},
				new
				{
					Name = "Erik Karlsson",
					Title = "Full Stack Developer",
					Skills = new[] { "React", "Node.js", "TypeScript", "MongoDB", "AWS", "Docker" },
					Experience = "7 years",
					Availability = "2 weeks",
					Rate = "1050 SEK/hour",
					Languages = new[] { "Swedish", "English" }
				},
				new
				{
					Name = "Maria Svensson",
					Title = "Solutions Architect",
					Skills = new[] { "Architecture", "Cloud Design", "Azure", "AWS", "Kubernetes", "DevOps", "C#", "Python" },
					Experience = "15 years",
					Availability = "1 month",
					Rate = "1500 SEK/hour",
					Languages = new[] { "Swedish", "English", "German" }
				}
			};

			// Sample assignment
			var assignment = @"
Vi söker en senior utvecklare för ett 6 månaders uppdrag.

Projekt: Modernisering av befintligt .NET Framework system till .NET 8
Krav:
- Minst 8 års erfarenhet av C# och .NET
- Erfarenhet av Azure
- Erfarenhet av microservices-arkitektur
- Goda kunskaper i svenska

Önskemål:
- Erfarenhet av REST API design
- Kännedom om SQL Server
- Kan börja inom 2 veckor

Budget: Max 1300 SEK/timme
Startdatum: Så snart som möjligt
Längd: 6 månader
Plats: Hybrid (Stockholm)
";

			WriteLine("ASSIGNMENT:");
			WriteLine(assignment);
			WriteLine();
			WriteLine("═══════════════════════════════════════");
			WriteLine();

			// Create agent with consultant database tool
			var agent = new Agent(_client!, new AgentOptions
			{
				Name = "consultant-matcher",
				Instructions = @"You are an AI consultant matching system. You MUST use the provided tools to get data.

CRITICAL RULES:
- You have EXACTLY TWO tools: get_consultants and calculate_match_score
- You MUST call get_consultants FIRST to retrieve consultant data
- DO NOT make up or generate consultant data yourself
- DO NOT create fake tool calls with consultant names
- ONLY call the actual tool names: get_consultants and calculate_match_score

WORKFLOW:
1. Call get_consultants with input=""all"" to retrieve the consultant list
2. Wait for the tool result (you will receive the actual consultant data)
3. In the SAME response, call calculate_match_score for ALL consultants at once (make multiple tool calls in one message)
4. After receiving all scores, analyze and provide recommendations

IMPORTANT: After step 2, you must call calculate_match_score for EVERY consultant in the list within the SAME iteration. Do NOT call them one at a time across multiple iterations.

TOOL DETAILS:

get_consultants:
- Call this FIRST with any input (e.g., ""all"")
- Returns: JSON array with consultant profiles
- Example: <function_calls><invoke name=""get_consultants""><parameter name=""input"">all</parameter></invoke></function_calls>

calculate_match_score:
- Call AFTER you have consultant data from get_consultants
- Requires: JSON string with 'consultantName' and 'requirements'
- Example: <function_calls><invoke name=""calculate_match_score""><parameter name=""input"">{""consultantName"": ""John Doe"", ""requirements"": ""C# developer""}</parameter></invoke></function_calls>

Remember: ALWAYS use the actual tools. NEVER generate fake data.",
				MaxIterations = 10,
				Model = "anthropic/claude-opus-4.5"
			})
			.WithTool(AgentTool.Create(
				name: "get_consultants",
				description: "Retrieves the complete list of available consultants with their full profiles including skills, experience, availability, rate, and languages. Returns JSON array.",
				handler: (string _) => // Takes a string parameter but we ignore it
				{
					return Task.FromResult(JsonSerializer.Serialize(consultants, new JsonSerializerOptions
					{
						WriteIndented = true
					}));
				},
				inputDescription: "Any query string (e.g., 'all', 'list', 'available'). The tool always returns all consultants."
			))
			.WithTool(AgentTool.Create(
				name: "calculate_match_score",
				description: "Calculates how well a specific consultant matches the job requirements. Returns a score from 0-100. Input must be valid JSON string with 'consultantName' and 'requirements' fields.",
				handler: (string input) =>
				{
					// Parse input JSON
					var inputObj = JsonSerializer.Deserialize<Dictionary<string, string>>(input);
					var consultantName = inputObj?.GetValueOrDefault("consultantName") ?? "";
					var requirements = inputObj?.GetValueOrDefault("requirements") ?? "";

					// Simple scoring logic (in reality, this would be more sophisticated)
					var consultant = consultants.FirstOrDefault(c => c.Name == consultantName);
					if (consultant == null) return Task.FromResult("0");

					int score = 50; // Base score

					// Check key skills
					if (consultant.Skills.Contains("C#")) score += 10;
					if (consultant.Skills.Contains(".NET 8") || consultant.Skills.Contains(".NET")) score += 10;
					if (consultant.Skills.Contains("Azure")) score += 10;
					if (consultant.Skills.Contains("Microservices")) score += 10;

					// Check experience
					if (int.TryParse(consultant.Experience.Split(' ')[0], out int years) && years >= 8) score += 5;

					// Check rate
					if (int.TryParse(consultant.Rate.Split(' ')[0], out int rate) && rate <= 1300) score += 5;

					return Task.FromResult(score.ToString());
				},
				inputDescription: "JSON string with format: {\"consultantName\": \"Full Name\", \"requirements\": \"description of requirements\"}"
			));

			WriteLine("Agent analyzing assignment and matching consultants...");
			WriteLine();

			var query = $@"
Analyze this assignment and find the best matching consultant:

{assignment}

For each consultant:
1. Get their profile
2. Calculate match score
3. Explain why they are or aren't a good fit

Provide a ranked recommendation with reasoning.";

			var response = await agent.RunAsync(query);

			WriteLine("═══════════════════════════════════════");
			WriteLine("AGENT RECOMMENDATION:");
			WriteLine("═══════════════════════════════════════");
			WriteLine(response.Output);
			WriteLine();
			WriteLine($"Analysis completed in {response.Iterations} iterations using {response.ToolCalls.Count} tool calls.");
		}
	static async Task TestScaleConsultantMatching()
	{
		WriteLine("═══════════════════════════════════════");
		WriteLine("SCALE TEST: 25 Consultants with Rich CV Data");
		WriteLine("═══════════════════════════════════════");
		WriteLine();
		WriteLine("This tests the agent's ability to handle a realistic number of consultants");
		WriteLine("with complex CV data including skills, projects, certifications, etc.");
		WriteLine();

		// Generate 25 realistic consultant profiles
		var consultants = new[]
		{
			new {
				Id = "C001", Name = "Anna Andersson", Title = "Senior .NET Developer",
				Skills = new[] { "C#", ".NET 8", "Azure", "Microservices", "REST API", "SQL Server", "Docker" },
				Experience = "10 years", Rate = "1200 SEK/h", Availability = "Immediate",
				Languages = new[] { "Swedish", "English" },
				Projects = new[] {
					"E-commerce platform migration to .NET 8 (2023-2024)",
					"Azure microservices architecture (2022-2023)",
					"Banking API development (2020-2022)"
				},
				Certifications = new[] { "Azure Solutions Architect", "Microsoft Certified: Azure Developer" }
			},
			new {
				Id = "C002", Name = "Erik Bergström", Title = "Full Stack Developer",
				Skills = new[] { "React", "Node.js", "TypeScript", "MongoDB", "AWS", "Docker", "GraphQL" },
				Experience = "7 years", Rate = "1050 SEK/h", Availability = "2 weeks",
				Languages = new[] { "Swedish", "English" },
				Projects = new[] {
					"SaaS dashboard development (2023-2024)",
					"E-learning platform (2021-2023)",
					"Mobile app backend (2020-2021)"
				},
				Certifications = new[] { "AWS Certified Developer" }
			},
			new {
				Id = "C003", Name = "Maria Carlsson", Title = "Solutions Architect",
				Skills = new[] { "Architecture", "Cloud Design", "Azure", "AWS", "Kubernetes", "DevOps", "C#", "Python" },
				Experience = "15 years", Rate = "1500 SEK/h", Availability = "1 month",
				Languages = new[] { "Swedish", "English", "German" },
				Projects = new[] {
					"Multi-cloud strategy design (2023-2024)",
					"Enterprise architecture modernization (2021-2023)",
					"DevOps transformation (2019-2021)"
				},
				Certifications = new[] { "TOGAF 9", "AWS Solutions Architect Professional", "Azure Solutions Architect Expert" }
			},
			new {
				Id = "C004", Name = "Lars Danielsson", Title = "Backend Developer",
				Skills = new[] { "Java", "Spring Boot", "PostgreSQL", "Kafka", "Kubernetes", "REST API" },
				Experience = "8 years", Rate = "1100 SEK/h", Availability = "3 weeks",
				Languages = new[] { "Swedish", "English" },
				Projects = new[] {
					"Payment processing system (2022-2024)",
					"Event-driven microservices (2020-2022)",
					"Legacy system modernization (2018-2020)"
				},
				Certifications = new[] { "Oracle Certified Professional" }
			},
			new {
				Id = "C005", Name = "Sofia Eriksson", Title = "Frontend Developer",
				Skills = new[] { "Vue.js", "React", "TypeScript", "CSS", "Webpack", "Jest" },
				Experience = "5 years", Rate = "950 SEK/h", Availability = "Immediate",
				Languages = new[] { "Swedish", "English" },
				Projects = new[] {
					"Design system implementation (2023-2024)",
					"Progressive web app (2022-2023)",
					"Component library (2021-2022)"
				},
				Certifications = new[] { "Google UX Design Professional" }
			},
			new {
				Id = "C006", Name = "Johan Fransson", Title = "DevOps Engineer",
				Skills = new[] { "Kubernetes", "Docker", "Jenkins", "Terraform", "Azure", "GitLab CI/CD", "Python" },
				Experience = "9 years", Rate = "1250 SEK/h", Availability = "2 weeks",
				Languages = new[] { "Swedish", "English" },
				Projects = new[] {
					"CI/CD pipeline automation (2023-2024)",
					"Infrastructure as Code migration (2021-2023)",
					"Container orchestration (2019-2021)"
				},
				Certifications = new[] { "Certified Kubernetes Administrator", "Azure DevOps Engineer" }
			},
			new {
				Id = "C007", Name = "Emma Gustafsson", Title = "Data Engineer",
				Skills = new[] { "Python", "Spark", "Databricks", "Azure Data Factory", "SQL", "Power BI" },
				Experience = "6 years", Rate = "1150 SEK/h", Availability = "1 week",
				Languages = new[] { "Swedish", "English" },
				Projects = new[] {
					"Data lake implementation (2023-2024)",
					"ETL pipeline development (2021-2023)",
					"Data warehouse migration (2020-2021)"
				},
				Certifications = new[] { "Databricks Certified Data Engineer" }
			},
			new {
				Id = "C008", Name = "Oscar Hansen", Title = "Mobile Developer",
				Skills = new[] { "Swift", "Kotlin", "React Native", "Flutter", "Firebase", "REST API" },
				Experience = "7 years", Rate = "1100 SEK/h", Availability = "3 weeks",
				Languages = new[] { "Swedish", "English" },
				Projects = new[] {
					"Cross-platform mobile app (2023-2024)",
					"iOS banking app (2021-2023)",
					"Android e-commerce app (2019-2021)"
				},
				Certifications = new[] { "Google Mobile Web Specialist" }
			},
			new {
				Id = "C009", Name = "Linnea Isaksson", Title = "Cloud Architect",
				Skills = new[] { "Azure", "AWS", "GCP", "Cloud Security", "Networking", "Terraform", "ARM Templates" },
				Experience = "12 years", Rate = "1400 SEK/h", Availability = "1 month",
				Languages = new[] { "Swedish", "English", "French" },
				Projects = new[] {
					"Multi-cloud governance (2022-2024)",
					"Cloud security framework (2020-2022)",
					"Hybrid cloud architecture (2018-2020)"
				},
				Certifications = new[] { "Azure Solutions Architect Expert", "AWS Solutions Architect Professional", "CCSP" }
			},
			new {
				Id = "C010", Name = "Viktor Johansson", Title = "QA Automation Engineer",
				Skills = new[] { "Selenium", "Cypress", "JUnit", "TestNG", "Jenkins", "C#", "Java" },
				Experience = "8 years", Rate = "1050 SEK/h", Availability = "2 weeks",
				Languages = new[] { "Swedish", "English" },
				Projects = new[] {
					"Test automation framework (2022-2024)",
					"CI/CD test integration (2020-2022)",
					"Performance testing (2018-2020)"
				},
				Certifications = new[] { "ISTQB Advanced Test Automation Engineer" }
			},
			new {
				Id = "C011", Name = "Klara Karlsson", Title = ".NET Architect",
				Skills = new[] { "C#", ".NET", "Azure", "Microservices", "DDD", "Event Sourcing", "CQRS" },
				Experience = "14 years", Rate = "1450 SEK/h", Availability = "6 weeks",
				Languages = new[] { "Swedish", "English" },
				Projects = new[] {
					"Enterprise architecture redesign (2022-2024)",
					"Domain-driven design implementation (2020-2022)",
					"Microservices migration (2018-2020)"
				},
				Certifications = new[] { "Microsoft Certified: Azure Solutions Architect Expert" }
			},
			new {
				Id = "C012", Name = "Nils Larsson", Title = "Security Engineer",
				Skills = new[] { "Security", "Penetration Testing", "OWASP", "Azure Security", "Python", "PowerShell" },
				Experience = "10 years", Rate = "1350 SEK/h", Availability = "4 weeks",
				Languages = new[] { "Swedish", "English" },
				Projects = new[] {
					"Security audit and remediation (2023-2024)",
					"Zero trust architecture (2021-2023)",
					"Threat modeling (2019-2021)"
				},
				Certifications = new[] { "CISSP", "CEH", "OSCP" }
			},
			new {
				Id = "C013", Name = "Olivia Lindström", Title = "Scrum Master",
				Skills = new[] { "Scrum", "Agile", "Jira", "Confluence", "Facilitation", "Coaching" },
				Experience = "6 years", Rate = "1000 SEK/h", Availability = "Immediate",
				Languages = new[] { "Swedish", "English" },
				Projects = new[] {
					"Agile transformation (2022-2024)",
					"Multi-team coordination (2020-2022)",
					"Process improvement (2019-2020)"
				},
				Certifications = new[] { "Certified Scrum Master", "SAFe Agilist" }
			},
			new {
				Id = "C014", Name = "Filip Magnusson", Title = "AI/ML Engineer",
				Skills = new[] { "Python", "TensorFlow", "PyTorch", "Azure ML", "MLOps", "NLP", "Computer Vision" },
				Experience = "5 years", Rate = "1300 SEK/h", Availability = "2 weeks",
				Languages = new[] { "Swedish", "English" },
				Projects = new[] {
					"Predictive maintenance ML model (2023-2024)",
					"NLP chatbot development (2022-2023)",
					"Image classification system (2021-2022)"
				},
				Certifications = new[] { "Azure AI Engineer Associate", "TensorFlow Developer" }
			},
			new {
				Id = "C015", Name = "Elin Nilsson", Title = "Product Owner",
				Skills = new[] { "Product Management", "Roadmapping", "Stakeholder Management", "Agile", "Data Analysis" },
				Experience = "8 years", Rate = "1150 SEK/h", Availability = "1 month",
				Languages = new[] { "Swedish", "English" },
				Projects = new[] {
					"Product strategy and execution (2022-2024)",
					"Feature prioritization (2020-2022)",
					"User research and validation (2018-2020)"
				},
				Certifications = new[] { "Certified Scrum Product Owner", "Product Management Certificate" }
			},
			new {
				Id = "C016", Name = "Gustav Olsson", Title = "Integration Specialist",
				Skills = new[] { "Azure Integration Services", "Logic Apps", "Service Bus", "API Management", "BizTalk", "C#" },
				Experience = "11 years", Rate = "1250 SEK/h", Availability = "3 weeks",
				Languages = new[] { "Swedish", "English" },
				Projects = new[] {
					"Enterprise integration platform (2022-2024)",
					"Legacy system integration (2020-2022)",
					"API gateway implementation (2018-2020)"
				},
				Certifications = new[] { "Microsoft Certified: Azure Integration Services" }
			},
			new {
				Id = "C017", Name = "Ida Persson", Title = "UX Designer",
				Skills = new[] { "UX Design", "UI Design", "Figma", "User Research", "Prototyping", "Accessibility" },
				Experience = "7 years", Rate = "1100 SEK/h", Availability = "2 weeks",
				Languages = new[] { "Swedish", "English" },
				Projects = new[] {
					"Design system creation (2023-2024)",
					"Mobile app redesign (2021-2023)",
					"Usability testing (2019-2021)"
				},
				Certifications = new[] { "Nielsen Norman Group UX Certification" }
			},
			new {
				Id = "C018", Name = "Alexander Pettersson", Title = "Site Reliability Engineer",
				Skills = new[] { "Kubernetes", "Prometheus", "Grafana", "Linux", "Python", "Go", "Incident Management" },
				Experience = "9 years", Rate = "1300 SEK/h", Availability = "4 weeks",
				Languages = new[] { "Swedish", "English" },
				Projects = new[] {
					"SRE practices implementation (2022-2024)",
					"Observability platform (2020-2022)",
					"High availability design (2018-2020)"
				},
				Certifications = new[] { "Google Cloud Professional Cloud Architect" }
			},
			new {
				Id = "C019", Name = "Maja Svensson", Title = "Business Analyst",
				Skills = new[] { "Requirements Analysis", "Process Modeling", "SQL", "Power BI", "Stakeholder Management" },
				Experience = "10 years", Rate = "1050 SEK/h", Availability = "Immediate",
				Languages = new[] { "Swedish", "English", "Spanish" },
				Projects = new[] {
					"Business process optimization (2022-2024)",
					"Requirements elicitation (2020-2022)",
					"System integration analysis (2018-2020)"
				},
				Certifications = new[] { "CBAP", "PMI-PBA" }
			},
			new {
				Id = "C020", Name = "Anton Söderberg", Title = "Platform Engineer",
				Skills = new[] { "Kubernetes", "Terraform", "AWS", "Azure", "GitOps", "Helm", "Prometheus" },
				Experience = "6 years", Rate = "1200 SEK/h", Availability = "1 week",
				Languages = new[] { "Swedish", "English" },
				Projects = new[] {
					"Internal developer platform (2023-2024)",
					"Multi-tenant architecture (2021-2023)",
					"Infrastructure automation (2020-2021)"
				},
				Certifications = new[] { "CKA", "AWS Solutions Architect" }
			},
			new {
				Id = "C021", Name = "Ebba Strömberg", Title = "Tech Lead",
				Skills = new[] { "Leadership", "C#", ".NET", "Azure", "Mentoring", "Architecture", "Agile" },
				Experience = "13 years", Rate = "1400 SEK/h", Availability = "2 months",
				Languages = new[] { "Swedish", "English" },
				Projects = new[] {
					"Team leadership and mentoring (2021-2024)",
					"Technical strategy (2019-2021)",
					"Architecture decisions (2017-2019)"
				},
				Certifications = new[] { "Microsoft Certified: Azure Solutions Architect Expert" }
			},
			new {
				Id = "C022", Name = "Hugo Wallin", Title = "Database Administrator",
				Skills = new[] { "SQL Server", "PostgreSQL", "MySQL", "Database Tuning", "Backup & Recovery", "Azure SQL" },
				Experience = "12 years", Rate = "1150 SEK/h", Availability = "3 weeks",
				Languages = new[] { "Swedish", "English" },
				Projects = new[] {
					"Database migration to Azure (2022-2024)",
					"Performance optimization (2020-2022)",
					"High availability setup (2018-2020)"
				},
				Certifications = new[] { "Microsoft Certified: Azure Database Administrator" }
			},
			new {
				Id = "C023", Name = "Alice Wikström", Title = "Blockchain Developer",
				Skills = new[] { "Solidity", "Ethereum", "Smart Contracts", "Web3", "Node.js", "React" },
				Experience = "4 years", Rate = "1250 SEK/h", Availability = "2 weeks",
				Languages = new[] { "Swedish", "English" },
				Projects = new[] {
					"DeFi platform development (2023-2024)",
					"NFT marketplace (2022-2023)",
					"Smart contract auditing (2021-2022)"
				},
				Certifications = new[] { "Certified Blockchain Developer" }
			},
			new {
				Id = "C024", Name = "Elias Öberg", Title = "Game Developer",
				Skills = new[] { "Unity", "C#", "Unreal Engine", "3D Graphics", "Game Design", "Multiplayer" },
				Experience = "8 years", Rate = "1100 SEK/h", Availability = "4 weeks",
				Languages = new[] { "Swedish", "English" },
				Projects = new[] {
					"Multiplayer game development (2022-2024)",
					"VR experience (2020-2022)",
					"Mobile game (2018-2020)"
				},
				Certifications = new[] { "Unity Certified Developer" }
			},
			new {
				Id = "C025", Name = "Stella Åström", Title = "API Developer",
				Skills = new[] { "REST API", "GraphQL", "Node.js", "Express", "MongoDB", "API Security", "Swagger" },
				Experience = "6 years", Rate = "1050 SEK/h", Availability = "1 week",
				Languages = new[] { "Swedish", "English" },
				Projects = new[] {
					"API gateway implementation (2023-2024)",
					"Microservices API (2021-2023)",
					"Third-party integrations (2019-2021)"
				},
				Certifications = new[] { "API Security Certified" }
			}
		};

		var assignment = @"
Vi söker en senior utvecklare för ett 6 månaders uppdrag.

Projekt: Modernisering av befintligt .NET Framework system till .NET 8
Krav:
- Minst 8 års erfarenhet av C# och .NET
- Erfarenhet av Azure
- Erfarenhet av microservices-arkitektur
- Goda kunskaper i svenska

Önskemål:
- Erfarenhet av REST API design
- Kännedom om SQL Server
- Kan börja inom 2 veckor

Budget: Max 1300 SEK/timme
Startdatum: Så snart som möjligt
Längd: 6 månader
Plats: Hybrid (Stockholm)
";

		WriteLine("ASSIGNMENT:");
		WriteLine(assignment);
		WriteLine();
		WriteLine($"Searching through {consultants.Length} consultants...");
		WriteLine("═══════════════════════════════════════");
		WriteLine();

		var agent = new Agent(_client!, new AgentOptions
		{
			Name = "consultant-matcher-scale",
			Instructions = @"EXECUTE THESE TOOLS NOW (do not write text, only make tool calls):

get_consultants: all
calculate_match_score: C001
calculate_match_score: C003
calculate_match_score: C006
calculate_match_score: C010
calculate_match_score: C011
calculate_match_score: C016
calculate_match_score: C021
calculate_match_score: C022

After tools finish, provide recommendations.

NO TEXT. ONLY TOOL CALLS. NOW.",
			MaxIterations = 15,
			Model = "anthropic/claude-opus-4.5"
		})
		.WithTool(AgentTool.Create(
			name: "get_consultants",
			description: "Retrieves ALL 25 consultant profiles with complete information",
			handler: (string input) =>
			{
				WriteLine($"   [DEBUG] get_consultants called with input: {input}");
				return Task.FromResult(JsonSerializer.Serialize(consultants, new JsonSerializerOptions { WriteIndented = true }));
			},
			inputDescription: "Any query string (e.g., 'all', 'list', 'available'). The tool always returns all consultants."
		))
		.WithTool(AgentTool.Create(
			name: "calculate_match_score",
			description: "Calculates match score (0-100) for ONE specific consultant",
			handler: (string input) =>
			{
				var inputObj = JsonSerializer.Deserialize<Dictionary<string, string>>(input);
				var consultantId = inputObj?.GetValueOrDefault("consultantId") ?? "";
				WriteLine($"   [DEBUG] calculate_match_score called for consultant: {consultantId}");

				var consultant = consultants.FirstOrDefault(c => c.Id == consultantId);
				if (consultant == null) return Task.FromResult("0");

				int score = 50;
				if (consultant.Skills.Contains("C#")) score += 10;
				if (consultant.Skills.Any(s => s.Contains(".NET"))) score += 10;
				if (consultant.Skills.Contains("Azure")) score += 10;
				if (consultant.Skills.Contains("Microservices")) score += 10;
				if (int.TryParse(consultant.Experience.Split(' ')[0], out int years) && years >= 8) score += 5;
				if (int.TryParse(consultant.Rate.Split(' ')[0], out int rate) && rate <= 1300) score += 5;

				return Task.FromResult(score.ToString());
			},
			inputDescription: "JSON string with format: {\"consultantId\": \"C001\", \"requirements\": \"description of requirements\"}"
		));

		var response = await agent.RunAsync($@"Find the best consultants for this assignment:

{assignment}

Remember: Get all consultants first, then score only the most promising ones.");

		WriteLine("═══════════════════════════════════════");
		WriteLine("AGENT RECOMMENDATION:");
		WriteLine("═══════════════════════════════════════");
		WriteLine(response.Output);
		WriteLine();
		WriteLine($"Performance: {response.Iterations} iterations, {response.ToolCalls.Count} tool calls for {consultants.Length} consultants");
	}

	static async Task TestErrorHandling()
	{
		WriteLine("═══════════════════════════════════════");
		WriteLine("ERROR HANDLING TEST: Tools with Failures");
		WriteLine("═══════════════════════════════════════");
		WriteLine();
		WriteLine("Tests agent resilience when tools fail, throw exceptions, or return errors.");
		WriteLine();

		var callCount = 0;
		var random = new Random();

		var agent = new Agent(_client!, new AgentOptions
		{
			Name = "error-handler",
			Instructions = @"Call these 3 tools immediately in your first response:

1. unreliable_data_fetch with input: ""customer_12345""
2. flaky_calculation with input: ""account balance for customer_12345""
3. slow_service with input: ""transaction history for customer_12345""

After the tools execute (some may fail), report what happened.

DO NOT explain or describe - JUST CALL THE 3 TOOLS NOW.",
			MaxIterations = 10,
			Model = "anthropic/claude-opus-4.5"
		})
		.WithTool(AgentTool.Create(
			name: "unreliable_data_fetch",
			description: "Fetches data but may fail (30% failure rate)",
			handler: (string query) =>
			{
				callCount++;
				if (random.Next(100) < 30)
					throw new Exception($"Network timeout (attempt #{callCount})");
				if (random.Next(100) < 20)
					return Task.FromResult($"{{\"status\":\"partial\",\"data\":\"Incomplete: {query}\"}}");
				return Task.FromResult($"{{\"status\":\"success\",\"data\":\"Results for {query}\"}}");
			},
			inputDescription: "Search query for data retrieval (e.g., 'customer_12345', 'user data')"
		))
		.WithTool(AgentTool.Create(
			name: "flaky_calculation",
			description: "Performs calculations but may fail (25% failure rate)",
			handler: (string expression) =>
			{
				callCount++;
				if (random.Next(100) < 25)
					throw new InvalidOperationException($"Service unavailable (attempt #{callCount})");
				return Task.FromResult($"Result: {random.Next(100, 1000)}");
			},
			inputDescription: "Mathematical expression or calculation request (e.g., 'account balance', '100 + 200')"
		))
		.WithTool(AgentTool.Create(
			name: "slow_service",
			description: "Slow service with 20% timeout rate",
			handler: async (string request) =>
			{
				callCount++;
				await Task.Delay(50);
				if (random.Next(100) < 20)
					throw new TimeoutException($"Timeout after 30s (attempt #{callCount})");
				return $"{{\"status\":\"ok\",\"data\":\"Info for {request}\"}}";
			},
			inputDescription: "Information request for slow service (e.g., 'transaction history', 'account details')"
		));

		var question = @"I need you to:
1. Fetch user data for 'customer_12345'
2. Calculate their account balance
3. Get transaction history

Handle any failures gracefully.";

		WriteLine($"Question: {question}");
		WriteLine();

		var response = await agent.RunAsync(question);

		WriteLine("═══════════════════════════════════════");
		WriteLine("AGENT RESPONSE:");
		WriteLine("═══════════════════════════════════════");
		WriteLine(response.Output);
		WriteLine();
		WriteLine($"Stats: {response.Iterations} iterations, {response.ToolCalls.Count} tool calls, {callCount} total attempts");

		var failures = response.ToolCalls.Count(c => {
			var r = c.Result?.ToString() ?? "";
			return r.Contains("ERROR") || r.Contains("timeout") || r.Contains("Failed");
		});
		WriteLine($"Success rate: {response.ToolCalls.Count - failures}/{response.ToolCalls.Count}");
	}
		// ═══════════════════════════════════════════════════════════════

		// ═══════════════════════════════════════════════════════════════
		// DEBUG UTILITIES
		// ═══════════════════════════════════════════════════════════════




		static async Task DeleteAllFunctions()
		{
			WriteLine("═══════════════════════════════════════");
			WriteLine("DELETE ALL FUNCTIONS");
			WriteLine("═══════════════════════════════════════");
			WriteLine();
			WriteLine("This will delete all 7 expected functions from your Opper account.");
			WriteLine("Use this to clean up before creating new ones.");
			WriteLine();

			var functionsToDelete = new[]
			{
			"math-solver",
			"research-agent",
			"problem-solver",
			"general-qa",
			"story-generator",
			"chat-assistant",
			"consultant-matcher"
		};

			WriteLine("Attempting to delete functions...");
			WriteLine();

			int successCount = 0;
			int notFoundCount = 0;
			int failCount = 0;

			foreach (var functionName in functionsToDelete)
			{
				Write($"  {functionName}... ");
				try
				{
					await _client!.Functions.DeleteAsync(functionName);
					WriteLine($"✅ DELETED");
					successCount++;
				}
				catch (OpperAPIException ex) when (ex.Message.Contains("NotFound") || ex.StatusCode == 404)
				{
					WriteLine($"⚠️  NOT FOUND (already deleted or never existed)");
					notFoundCount++;
				}
				catch (Exception ex)
				{
					WriteLine($"❌ FAILED: {ex.Message}");
					failCount++;
				}
			}

			WriteLine();
			WriteLine("═══════════════════════════════════════");
			WriteLine($"Results: {successCount} deleted, {notFoundCount} not found, {failCount} failed");
			WriteLine("═══════════════════════════════════════");
			WriteLine();

			if (successCount > 0)
			{
				WriteLine("✅ Functions deleted successfully!");
				WriteLine("   You can now run menu option 97 to create fresh functions.");
			}

			if (notFoundCount > 0)
			{
				WriteLine();
				WriteLine($"⚠️  {notFoundCount} function(s) were not found (already deleted or never existed).");
			}

			if (failCount > 0)
			{
				WriteLine();
				WriteLine($"❌ {failCount} function(s) failed to delete.");
				WriteLine("   Check the error messages above for details.");
			}
		}

		static async Task CreateAllFunctions()
		{
			WriteLine("═══════════════════════════════════════");
			WriteLine("CREATE ALL FUNCTIONS VIA API");
			WriteLine("═══════════════════════════════════════");
			WriteLine();
			WriteLine("This will create all 7 functions programmatically via the Opper API.");
			WriteLine("Functions created via Dashboard don't work - they must be created via API!");
			WriteLine();

			var functionsToCreate = new[]
			{
			new OpperFunctionDefinition
			{
				Path = "math-solver",
				Name = "math-solver",
				Description = "Math solver agent with calculation tools",
				Instructions = "You are a helpful math solver. You have access to calculation tools. Use them to solve mathematical problems step by step.",
				Model = "anthropic/claude-opus-4.5"
			},
			new OpperFunctionDefinition
			{
				Path = "research-agent",
				Name = "research-agent",
				Description = "Research agent with database and weather tools",
				Instructions = "You are a research agent with access to database queries and weather information. Help users find information.",
				Model = "anthropic/claude-opus-4.5"
			},
			new OpperFunctionDefinition
			{
				Path = "problem-solver",
				Name = "problem-solver",
				Description = "Multi-step problem solver for complex reasoning",
				Instructions = "You are a problem solver that can handle complex multi-step problems. Break down problems and solve them systematically.",
				Model = "anthropic/claude-opus-4.5"
			},
			new OpperFunctionDefinition
			{
				Path = "general-qa",
				Name = "general-qa",
				Description = "General Q&A function for simple questions",
				Instructions = "You are a helpful assistant that answers general questions clearly and concisely.",
				Model = "anthropic/claude-sonnet-4"
			},
			new OpperFunctionDefinition
			{
				Path = "story-generator",
				Name = "story-generator",
				Description = "Creative story generator for TestConsole",
				Instructions = "You are a creative writer. Generate engaging short stories based on the given topic.",
				Model = "anthropic/claude-sonnet-4.5"
			},
			new OpperFunctionDefinition
			{
				Path = "chat-assistant",
				Name = "chat-assistant",
				Description = "Conversational chat assistant",
				Instructions = @"You are a helpful AI assistant for conversational chat.

Guidelines:
- Be friendly and concise
- Consider the conversation history when responding
- Ask clarifying questions when needed
- Provide helpful, relevant answers
- Maintain context throughout the conversation

The user will provide:
- ""message"": Their current message
- ""history"": Previous conversation (if any)

Respond naturally and conversationally.",
				Model = "anthropic/claude-sonnet-4.5"
			},
			new OpperFunctionDefinition
			{
				Path = "consultant-matcher",
				Name = "consultant-matcher",
				Description = "Consultant matching system for assignments",
				Instructions = @"You are an AI consultant matching system. Analyze assignments and match them with the best consultants.

For each consultant:
1. Get their profile using the get_consultants tool
2. Calculate match score using the calculate_match_score tool
3. Explain why they are or aren't a good fit

Provide a ranked recommendation with clear reasoning.",
				Model = "anthropic/claude-opus-4.5"
			}
		};

			WriteLine("Creating functions...");
			WriteLine();

			int successCount = 0;
			int failCount = 0;

			foreach (var functionDef in functionsToCreate)
			{
				Write($"  {functionDef.Path}... ");
				try
				{
					var created = await _client!.Functions.CreateAsync(functionDef);
					WriteLine($"✅ CREATED (ID: {created.Id})");
					successCount++;
				}
				catch (Exception ex)
				{
					WriteLine($"❌ FAILED: {ex.Message}");
					failCount++;
				}
			}

			WriteLine();
			WriteLine("═══════════════════════════════════════");
			WriteLine($"Results: {successCount} created, {failCount} failed");
			WriteLine("═══════════════════════════════════════");
			WriteLine();

			if (successCount > 0)
			{
				WriteLine("✅ Functions created successfully!");
				WriteLine("   Run menu option 99 to verify they appear in the API.");
				WriteLine("   Then run menu options 1-6, 10 to test them!");
			}

			if (failCount > 0)
			{
				WriteLine();
				WriteLine("⚠️  Some functions failed to create.");
				WriteLine("   They may already exist. Try deleting them first.");
			}
		}

		static async Task InitializeFunctions()
		{
			WriteLine("═══════════════════════════════════════");
			WriteLine("INITIALIZE ALL FUNCTIONS");
			WriteLine("═══════════════════════════════════════");
			WriteLine();
			WriteLine("This will call each function once to initialize/activate it.");
			WriteLine("After this, they should appear in the API list.");
			WriteLine();

			var functionsToInitialize = new[]
			{
			("math-solver", new Dictionary<string, object> { ["problem"] = "What is 2+2?" }),
			("research-agent", new Dictionary<string, object> { ["query"] = "test" }),
			("problem-solver", new Dictionary<string, object> { ["problem"] = "test" }),
			("general-qa", new Dictionary<string, object> { ["question"] = "What is AI?" }),
			("story-generator", new Dictionary<string, object> { ["topic"] = "test" }),
			("chat-assistant", new Dictionary<string, object> { ["message"] = "Hello" }),
			("consultant-matcher", new Dictionary<string, object> { ["assignment"] = "test" })
		};

			WriteLine("Attempting to call each function...");
			WriteLine();

			int successCount = 0;
			int failCount = 0;

			foreach (var (functionName, input) in functionsToInitialize)
			{
				Write($"  {functionName}... ");
				try
				{
					var response = await _client!.Functions.CallAsync(
						path: functionName,
						input: input
					);

					if (!string.IsNullOrEmpty(response.Message))
					{
						WriteLine($"✅ SUCCESS (got response)");
						successCount++;
					}
					else
					{
						WriteLine($"⚠️  RESPONDED (but empty message)");
						successCount++;
					}
				}
				catch (Exception ex)
				{
					WriteLine($"❌ FAILED: {ex.Message}");
					failCount++;
				}
			}

			WriteLine();
			WriteLine("═══════════════════════════════════════");
			WriteLine($"Results: {successCount} succeeded, {failCount} failed");
			WriteLine("═══════════════════════════════════════");
			WriteLine();

			if (successCount > 0)
			{
				WriteLine("✅ Functions that succeeded should now be active in the API!");
				WriteLine("   Run menu option 99 to verify they appear in the list.");
			}

			if (failCount > 0)
			{
				WriteLine();
				WriteLine("⚠️  Failed functions were not found by the API.");
				WriteLine("   They may not have been created correctly in Opper Dashboard,");
				WriteLine("   or they may have different names than expected.");
			}
		}

		static async Task TestAdHocFunctionCall()
		{
			WriteLine("═══════════════════════════════════════");
			WriteLine("TEST: Ad-Hoc Function Call");
			WriteLine("═══════════════════════════════════════");
			WriteLine();
			WriteLine("This tests calling a function WITHOUT creating it first.");
			WriteLine("All function config (name, instructions, model) is sent in the request body.");
			WriteLine();

			try
			{
				var apiKey = _configuration?["OPPER_API_KEY"];
				using var httpClient = new HttpClient();
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
				httpClient.BaseAddress = new Uri("https://api.opper.ai/v2/");

				var requestBody = new
				{
					name = "math-solver-adhoc",
					instructions = "You are a helpful math solver. Answer the math question.",
					input = new { problem = "What is 10 + 5?" },
					model = "anthropic/claude-opus-4.5"
				};

				var jsonContent = new StringContent(
					Newtonsoft.Json.JsonConvert.SerializeObject(requestBody),
					System.Text.Encoding.UTF8,
					"application/json"
				);

				WriteLine("Sending ad-hoc call to POST /v2/call");
				WriteLine($"Request body: {Newtonsoft.Json.JsonConvert.SerializeObject(requestBody, Newtonsoft.Json.Formatting.Indented)}");
				WriteLine();

				var response = await httpClient.PostAsync("call", jsonContent);
				var responseContent = await response.Content.ReadAsStringAsync();

				WriteLine($"Response Status: {response.StatusCode}");
				WriteLine($"Response: {responseContent}");
				WriteLine();

				if (response.IsSuccessStatusCode)
				{
					WriteLine("✓ Ad-hoc function call SUCCEEDED!");
					WriteLine();
					WriteLine("This proves ad-hoc calls work, but named function calls don't.");
				}
				else
				{
					WriteLine("❌ Ad-hoc function call also failed.");
				}
			}
			catch (Exception ex)
			{
				WriteLine($"❌ Exception: {ex.GetType().Name}");
				WriteLine($"   Message: {ex.Message}");
			}
		}

		static async Task DebugGetFunction()
		{
			WriteLine("═══════════════════════════════════════");
			WriteLine("DEBUG: Get Function Details");
			WriteLine("═══════════════════════════════════════");
			WriteLine();

			try
			{
				// First, list all functions to find math-solver's UUID
				WriteLine("Step 1: Listing all functions to find UUID...");
				var functions = await _client!.Functions.ListAsync();

				var mathSolver = functions.FirstOrDefault(f => f.Path == "math-solver" || f.Name == "math-solver");

				if (mathSolver == null)
				{
					WriteLine("❌ math-solver not found in function list!");
					return;
				}

				WriteLine($"✓ Found math-solver:");
				WriteLine($"  ID (UUID): {mathSolver.Id}");
				WriteLine($"  Path: {mathSolver.Path}");
				WriteLine($"  Name: {mathSolver.Name}");
				WriteLine();

				// Try GET with UUID
				WriteLine("═══════════════════════════════════════");
				WriteLine("Step 2: GET function details using UUID...");
				WriteLine("═══════════════════════════════════════");
				WriteLine();

				var function = await _client!.Functions.GetAsync(mathSolver.Id);

				WriteLine("✓ Function retrieved successfully!");
				WriteLine();
				WriteLine($"  ID: {function.Id}");
				WriteLine($"  Path: {function.Path}");
				WriteLine($"  Name: {function.Name}");
				WriteLine($"  Description: {function.Description}");
				WriteLine($"  Model: {function.Model}");
				WriteLine($"  Instructions: {function.Instructions?.Substring(0, Math.Min(100, function.Instructions?.Length ?? 0))}...");
				WriteLine($"  Created: {function.CreatedAt}");
				WriteLine($"  Updated: {function.UpdatedAt}");
				WriteLine();

				if (function.InputSchema != null)
				{
					WriteLine($"  Input Schema: {function.InputSchema}");
					WriteLine();
				}

				if (function.IndexIds != null && function.IndexIds.Count > 0)
				{
					WriteLine($"  Index IDs: {string.Join(", ", function.IndexIds)}");
					WriteLine();
				}

				// Now try calling with UUID
				WriteLine("═══════════════════════════════════════");
				WriteLine("Step 3: CALL function using UUID...");
				WriteLine("═══════════════════════════════════════");
				WriteLine();

				var input = new Dictionary<string, object>
				{
					["problem"] = "What is 10 + 5?"
				};

				WriteLine($"Calling with UUID: {mathSolver.Id}");
				WriteLine($"Input: {Newtonsoft.Json.JsonConvert.SerializeObject(input)}");
				WriteLine();

				var result = await _client!.Functions.CallAsync(
					mathSolver.Id,  // Use UUID instead of path
					input,
					cancellationToken: default
				);

				WriteLine("✓ Function call with UUID succeeded!");
				WriteLine();
				WriteLine($"  Message: {result.Message}");
				WriteLine($"  Output: {result.Output}");
				WriteLine($"  Cached: {result.Cached}");
				if (result.Usage != null)
				{
					WriteLine($"  Tokens: {result.Usage.TotalTokens} (prompt: {result.Usage.PromptTokens}, completion: {result.Usage.CompletionTokens})");
				}

				// Now try calling with PATH to compare
				WriteLine();
				WriteLine("═══════════════════════════════════════");
				WriteLine("Step 4: CALL function using PATH (for comparison)...");
				WriteLine("═══════════════════════════════════════");
				WriteLine();

				WriteLine($"Calling with PATH: {mathSolver.Path}");
				WriteLine($"Input: {Newtonsoft.Json.JsonConvert.SerializeObject(input)}");
				WriteLine();

				var result2 = await _client!.Functions.CallAsync(
					mathSolver.Path,  // Try with path
					input,
					cancellationToken: default
				);

				WriteLine("✓ Function call with PATH succeeded!");
				WriteLine();
				WriteLine($"  Message: {result2.Message}");
				WriteLine($"  Output: {result2.Output}");
				WriteLine($"  Cached: {result2.Cached}");
				if (result2.Usage != null)
				{
					WriteLine($"  Tokens: {result2.Usage.TotalTokens} (prompt: {result2.Usage.PromptTokens}, completion: {result2.Usage.CompletionTokens})");
				}
			}
			catch (OpperAPIException ex)
			{
				WriteLine($"❌ API Exception: {ex.StatusCode}");
				WriteLine($"   Message: {ex.Message}");
				WriteLine($"   Response: {ex.ResponseContent}");
			}
			catch (Exception ex)
			{
				WriteLine($"❌ Exception: {ex.GetType().Name}");
				WriteLine($"   Message: {ex.Message}");
			}
		}

		static async Task DebugListFunctions()
		{
			WriteLine("═══════════════════════════════════════");
			WriteLine("DEBUG: List All Functions");
			WriteLine("═══════════════════════════════════════");
			WriteLine();
			WriteLine("Fetching all functions from your Opper account...");
			WriteLine();

			try
			{
				// Make raw API call to see what we actually get
				WriteLine("🔍 RAW API CALL:");
				var apiKey = _configuration?["OPPER_API_KEY"];

				using var httpClient = new HttpClient();
				httpClient.DefaultRequestHeaders.Authorization =
					new AuthenticationHeaderValue("Bearer", apiKey);

				var response = await httpClient.GetAsync("https://api.opper.ai/v2/functions");
				var rawJson = await response.Content.ReadAsStringAsync();

				WriteLine("Response Status: " + response.StatusCode);
				WriteLine("Raw JSON Response:");
				WriteLine(rawJson);
				WriteLine();
				WriteLine("═══════════════════════════════════════");
				WriteLine();

				var functions = await _client!.Functions.ListAsync();

				if (functions.Count == 0)
				{
					WriteLine("❌ NO FUNCTIONS FOUND!");
					WriteLine();
					WriteLine("This means the functions were not created successfully in Opper Dashboard.");
					WriteLine("Please go back to https://platform.opper.ai and verify:");
					WriteLine("1. Functions section exists");
					WriteLine("2. You created the 7 functions");
					WriteLine("3. Functions were saved successfully");
				}
				else
				{
					WriteLine($"✓ Found {functions.Count} function(s):");
					WriteLine();

					var expectedFunctions = new[]
					{
						"math-solver",
						"research-agent",
						"problem-solver",
						"general-qa",
						"story-generator",
						"chat-assistant",
						"consultant-matcher"
					};

					foreach (var func in functions)
					{
						var isExpected = Array.Exists(expectedFunctions, f => f == func.Path);
						var marker = isExpected ? "✓" : " ";
						WriteLine($"{marker} Path: {func.Path}");
						WriteLine($"  ID: {func.Id}");
						if (!string.IsNullOrEmpty(func.Description))
							WriteLine($"  Description: {func.Description}");
						WriteLine();
					}

					WriteLine("═══════════════════════════════════════");
					WriteLine("Expected functions for TestConsole:");
					WriteLine("═══════════════════════════════════════");
					foreach (var expected in expectedFunctions)
					{
						var found = functions.Exists(f => f.Path == expected);
						var marker = found ? "✓" : "❌";
						WriteLine($"{marker} {expected}");
					}
				}
			}
			catch (Exception ex)
			{
				WriteLine($"❌ ERROR: {ex.Message}");
				WriteLine();
				WriteLine("Stack trace:");
				WriteLine(ex.StackTrace);
			}
		}

		// HELPER CLASSES
		// ═══════════════════════════════════════════════════════════════

		public class MathTools
		{
			[Tool("Adds two numbers together")]
			public double Add(double a, double b)
			{
				return a + b;
			}

			[Tool("Multiplies two numbers")]
			public double Multiply(double a, double b)
			{
				return a * b;
			}

			[Tool("Calculates percentage of a value")]
			public double Percentage(double number, double percentage)
			{
				return number * (percentage / 100.0);
			}

			[Tool("Divides first number by second number")]
			public double Divide(double a, double b)
			{
				if (b == 0) throw new ArgumentException("Cannot divide by zero");
				return a / b;
			}

			[Tool("Subtracts second number from first number")]
			public double Subtract(double a, double b)
			{
				return a - b;
			}
		}
	}
}
