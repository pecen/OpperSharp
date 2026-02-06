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

			var response = await _client!.Functions.CallAsync(
				path: "general-qa", // You need to create this function in Opper
				input: new Dictionary<string, object>
				{
					["question"] = question
				},
				options: new OpperCallOptions
				{
					Model = "gpt-4",
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

			WriteLine("AI: ");
			Write("    ");

			await foreach (var chunk in _client!.Functions.CallStreamAsync(
				path: "story-generator", // You need to create this in Opper
				input: new Dictionary<string, object>
				{
					["topic"] = topic
				}
			))
			{
				if (chunk.Delta != null)
				{
					Write(chunk.Delta);
				}
			}

			WriteLine();
		}

		static async Task TestChatAPI()
		{
			WriteLine("═══════════════════════════════════════");
			WriteLine("TEST: Conversational Chat (via Functions)");
			WriteLine("═══════════════════════════════════════");
			WriteLine();
			WriteLine("Simple conversational chat. Type 'quit' to exit.");
			WriteLine("NOTE: This uses a 'chat-assistant' function in Opper.");
			WriteLine();

			var conversationHistory = new List<string>();

			while (true)
			{
				Write("You: ");
				var input = ReadLine();
				if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "quit")
					break;

				conversationHistory.Add($"User: {input}");

				var response = await _client!.Functions.CallAsync(
					path: "chat-assistant",
					input: new Dictionary<string, object>
					{
						["message"] = input,
						["history"] = string.Join("\n", conversationHistory.Take(conversationHistory.Count - 1))
					},
					options: new OpperCallOptions
					{
						Model = "gpt-4",
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
				Instructions = @"You are an AI consultant matching system that helps match consultants with job assignments.

You have access to TWO tools:

1. get_consultants - Returns a JSON array of all available consultants with their profiles (name, title, skills, experience, availability, rate, languages). Call this with any input (e.g., 'all' or 'list').

2. calculate_match_score - Calculates a match score (0-100) for a specific consultant against requirements. Input must be JSON with 'consultantName' and 'requirements' fields.

Your task:
1. First, get all consultants using get_consultants
2. For each consultant, calculate their match score using calculate_match_score
3. Analyze the scores and consultant profiles
4. Provide a ranked recommendation with clear reasoning explaining why each consultant is or isn't a good fit

Be thorough in your analysis and consider all factors: skills, experience, availability, rate, and language requirements.",
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
