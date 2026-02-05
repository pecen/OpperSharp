using OpperSharp.Core;
using OpperSharp.Agents;
using OpperSharp.Models.Functions;
using OpperSharp.Models.Knowledge;
using OpperSharp.Models.Embeddings;
using OpperSharp.Models.Models;
using System.Text.Json;
using static System.Console;

namespace OpperSharp.UI.TestConsole
{
	internal class Program
	{
		private static OpperClient? _client;

		static async Task Main(string[] args)
		{
			try
			{
				// Initialize client
				_client = OpperClient.FromEnvironment();
				WriteLine("✓ OpperClient initialized from environment variable OPPER_API_KEY");
				WriteLine();
			}
			catch (Exception ex)
			{
				WriteLine("ERROR: Could not initialize OpperClient.");
				WriteLine("Make sure OPPER_API_KEY environment variable is set.");
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
					var key = ReadKey().KeyChar;
					WriteLine("\n");

					switch (key)
					{
						// AGENT TESTS (Priority)
						case '1': await TestAgentBasicMath(); break;
						case '2': await TestAgentWithCustomTools(); break;
						case '3': await TestAgentMultiStep(); break;

						// BASIC API TESTS
						case '4': await TestSimpleFunctionCall(); break;
						case '5': await TestStreaming(); break;
						case '6': await TestChatAPI(); break;

						// v2 API TESTS
						case '7': await TestKnowledgeBase(); break;
						case '8': await TestEmbeddings(); break;
						case '9': await TestModelAliases(); break;

						// CONSULTANT MATCHING (End goal)
						case 'c':
						case 'C': await TestConsultantMatching(); break;

						// DEBUG
						case 'd':
						case 'D': await DebugListFunctions(); break;

						case '0': WriteLine("Goodbye!"); return;

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
			WriteLine("  C) Match Consultant to Assignment");
			WriteLine();
			WriteLine("  DEBUG:");
			WriteLine("  D) List all Functions in Opper account");
			WriteLine();
			WriteLine("  0) Exit");
			WriteLine();
			Write("  Select option > ");
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

			// Create agent with math tools
			var agent = new Agent(_client!, new AgentOptions
			{
				FunctionPath = "math-solver",
				MaxIterations = 10,
				EnableTracing = true,
				Model = "gpt-4"
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
				FunctionPath = "research-agent",
				MaxIterations = 10
			})
			.WithTool(AgentTool.Create(
				name: "query_database",
				description: "Query user database for information about users",
				handler: (string query) =>
				{
					// Simulate database query
					return Task.FromResult(JsonSerializer.Serialize(new[]
					{
						new { Name = "Anna Andersson", Role = "Senior Developer", Skills = new[] { "C#", ".NET", "Azure" } },
						new { Name = "Erik Eriksson", Role = "Tech Lead", Skills = new[] { "Python", "AI/ML", "AWS" } },
						new { Name = "Maria Svensson", Role = "Architect", Skills = new[] { "Architecture", "Microservices", "Kubernetes" } }
					}));
				}
			))
			.WithTool(AgentTool.Create(
				name: "get_weather",
				description: "Get current weather for a city",
				handler: (string city) =>
				{
					// Simulate weather API
					var temp = new Random().Next(15, 25);
					return Task.FromResult($"Temperature in {city}: {temp}°C, Partly cloudy");
				}
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
				FunctionPath = "problem-solver",
				MaxIterations = 15,
				EnableTracing = true
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
				FunctionPath = "consultant-matcher",
				MaxIterations = 10,
				Model = "gpt-4"
			})
			.WithTool(AgentTool.Create(
				name: "get_consultants",
				description: "Retrieves list of available consultants with their profiles",
				handler: (string _) => // Takes a string parameter but we ignore it
				{
					return Task.FromResult(JsonSerializer.Serialize(consultants, new JsonSerializerOptions
					{
						WriteIndented = true
					}));
				}
			))
			.WithTool(AgentTool.Create(
				name: "calculate_match_score",
				description: "Calculate how well a consultant matches the requirements (0-100). Input should be JSON with consultantName and requirements fields.",
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
				}
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
			public double Percentage(double value, double percent)
			{
				return value * (percent / 100.0);
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
