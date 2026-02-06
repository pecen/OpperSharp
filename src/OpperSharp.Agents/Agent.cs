using OpperSharp.Core;
using OpperSharp.Models.Functions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace OpperSharp.Agents
{
	/// <summary>
	/// An AI agent that can use tools to accomplish tasks.
	/// </summary>
	public class Agent
	{
		private readonly OpperClient _client;
		private readonly AgentOptions _options;

		/// <summary>
		/// Creates a new agent with the given client and options.
		/// </summary>
		public Agent(OpperClient client, AgentOptions options)
		{
			_client = client ?? throw new ArgumentNullException(nameof(client));
			_options = options ?? throw new ArgumentNullException(nameof(options));

			// Require either Name (for ad-hoc calls) or FunctionPath (for legacy named function calls)
			if (string.IsNullOrWhiteSpace(_options.Name) && string.IsNullOrWhiteSpace(_options.FunctionPath))
				throw new ArgumentException("Either Name or FunctionPath is required", nameof(options));

			// If using ad-hoc mode (Name without FunctionPath), Instructions should be provided
			if (!string.IsNullOrWhiteSpace(_options.Name) && string.IsNullOrWhiteSpace(_options.FunctionPath)
				&& string.IsNullOrWhiteSpace(_options.Instructions))
			{
				throw new ArgumentException("Instructions are required when using ad-hoc mode (Name without FunctionPath)", nameof(options));
			}
		}

		/// <summary>
		/// Runs the agent with the given input.
		/// </summary>
		public async Task<AgentResponse> RunAsync(
			Dictionary<string, object> input,
			CancellationToken cancellationToken = default)
		{
			var response = new AgentResponse
			{
				Success = false,
				ToolCalls = new List<ToolCall>()
			};

			string? currentSpanId = null;

			try
			{
				// Create parent span if tracing is enabled
				if (_options.EnableTracing)
				{
					var agentName = _options.Name ?? _options.FunctionPath ?? "agent";
					var span = await _client.Spans.CreateAsync(
						$"agent:{agentName}",
						input,
						_options.ParentSpanId,
						_options.Metadata,
						cancellationToken
					);
					currentSpanId = span.Id;
					response.SpanId = span.Id;
					response.TraceId = span.TraceId;
				}

				var currentInput = new Dictionary<string, object>(input);
				var originalQuestion = input.ContainsKey("input") ? input["input"]?.ToString() : null;
				var iteration = 0;

				while (iteration < _options.MaxIterations)
				{
					iteration++;
					response.Iterations = iteration;

					// Call the function with current input
					var callOptions = new OpperCallOptions
					{
						Name = _options.Name ?? _options.FunctionPath ?? "agent",
						Instructions = _options.Instructions,
						Context = _options.Context,
						ParentSpanId = currentSpanId,
						Model = _options.Model,
						Temperature = _options.Temperature,
						Metadata = _options.Metadata,
						Tools = ConvertToolsToApiFormat(_options.Tools)
					};

					// DEBUG: Log what's being sent
					var callMode = string.IsNullOrWhiteSpace(_options.FunctionPath) ? "ad-hoc" : "named";
					System.Console.WriteLine($"[DEBUG] Iteration {iteration}: Calling {callOptions.Name} ({callMode} mode)");
					System.Console.WriteLine($"[DEBUG] Tools count: {callOptions.Tools?.Count ?? 0}");
					if (callOptions.Tools != null && callOptions.Tools.Count > 0)
					{
						System.Console.WriteLine($"[DEBUG] First tool: {Newtonsoft.Json.JsonConvert.SerializeObject(callOptions.Tools[0])}");
					}

					OpperFunctionResponse functionResponse;
					try
					{
						// Use null for path to trigger ad-hoc mode (name/instructions from options)
						// Or use FunctionPath for backward compatibility with named functions
						functionResponse = await _client.Functions.CallAsync(
							string.IsNullOrWhiteSpace(_options.FunctionPath) ? null : _options.FunctionPath,
							currentInput,
							callOptions,
							cancellationToken
						);
						System.Console.WriteLine($"[DEBUG] Function call succeeded");
					}
					catch (Exception ex)
					{
						System.Console.WriteLine($"[DEBUG] Function call FAILED: {ex.GetType().Name}: {ex.Message}");
						throw;
					}

					// DEBUG: Log response
					System.Console.WriteLine($"[DEBUG] Response Output is null: {functionResponse.Output == null}");
					System.Console.WriteLine($"[DEBUG] Response Output count: {functionResponse.Output?.Count ?? 0}");
					if (functionResponse.Output != null && functionResponse.Output.Count > 0)
					{
						System.Console.WriteLine($"[DEBUG] Response Output keys: {string.Join(", ", functionResponse.Output.Properties().Select(p => p.Name))}");
						System.Console.WriteLine($"[DEBUG] Response Output JSON: {functionResponse.Output.ToString(Newtonsoft.Json.Formatting.None)}");
					}
					System.Console.WriteLine($"[DEBUG] Response has tool_calls in Output: {functionResponse.Output?.ContainsKey("tool_calls") ?? false}");
					System.Console.WriteLine($"[DEBUG] Response message length: {functionResponse.Message?.Length ?? 0}");
					System.Console.WriteLine($"[DEBUG] Response message preview: {functionResponse.Message?.Substring(0, Math.Min(200, functionResponse.Message?.Length ?? 0))}");

					// Check if the response indicates tool calls
					// First try structured format (Output.tool_calls)
					bool hasToolCalls = false;
					List<Dictionary<string, object>>? toolCallsList = null;

					if (functionResponse.Output?.TryGetValue("tool_calls", out var toolCallsToken) == true
						&& toolCallsToken is JArray toolCallsFromOutput
						&& toolCallsFromOutput.Count > 0)
					{
						hasToolCalls = true;
						toolCallsList = new List<Dictionary<string, object>>();
						foreach (var tc in toolCallsFromOutput)
						{
							var dict = tc.ToObject<Dictionary<string, object>>();
							if (dict != null) toolCallsList.Add(dict);
						}
						System.Console.WriteLine($"[DEBUG] Found {toolCallsList.Count} tool calls in Output.tool_calls (structured format)");
					}
					// If not in Output, try parsing from message (Claude's native XML format)
					else if (!string.IsNullOrEmpty(functionResponse.Message))
					{
						toolCallsList = ParseToolCallsFromMessage(functionResponse.Message);
						if (toolCallsList.Count > 0)
						{
							hasToolCalls = true;
							System.Console.WriteLine($"[DEBUG] Found {toolCallsList.Count} tool calls in message (XML format)");
						}
					}

					if (hasToolCalls && toolCallsList != null && toolCallsList.Count > 0)
					{
						// Execute tool calls
						var toolResults = new List<object>();

						foreach (var toolCall in toolCallsList)
						{
							System.Console.WriteLine($"[DEBUG] Raw toolCall keys: {string.Join(", ", toolCall.Keys)}");
							var toolName = toolCall.GetValueOrDefault("name")?.ToString();
							var toolArgs = toolCall.GetValueOrDefault("arguments");

							System.Console.WriteLine($"[DEBUG] Extracted tool name: '{toolName}'");
							System.Console.WriteLine($"[DEBUG] Extracted tool args type: {toolArgs?.GetType().Name ?? "null"}");

							if (string.IsNullOrEmpty(toolName))
							{
								System.Console.WriteLine($"[DEBUG] Skipping tool call - name is null or empty");
								continue;
							}

							Dictionary<string, object?>? argsDict = null;
							if (toolArgs is JObject jobj)
							{
								argsDict = jobj.ToObject<Dictionary<string, object?>>();
								var keys = argsDict != null ? string.Join(", ", argsDict.Keys) : "";
								System.Console.WriteLine($"[DEBUG] Converted JObject to dictionary with {argsDict?.Count ?? 0} keys: {keys}");
							}
							else if (toolArgs is Dictionary<string, object> dict)
							{
								argsDict = dict.ToDictionary(kv => kv.Key, kv => (object?)kv.Value);
								System.Console.WriteLine($"[DEBUG] Using dictionary with {argsDict.Count} keys: {string.Join(", ", argsDict.Keys)}");
							}

							if (argsDict != null && argsDict.Count > 0)
							{
								foreach (var kvp in argsDict)
								{
									var valuePreview = kvp.Value?.ToString();
									if (valuePreview != null && valuePreview.Length > 100)
										valuePreview = valuePreview.Substring(0, 100) + "...";
									System.Console.WriteLine($"[DEBUG]   arg '{kvp.Key}' = '{valuePreview}' (type: {kvp.Value?.GetType().Name ?? "null"})");
								}
							}
							else
							{
								System.Console.WriteLine($"[DEBUG] No arguments extracted!");
							}

							try
							{
								var result = await ExecuteToolAsync(
									toolName,
									argsDict ?? new Dictionary<string, object?>(),
									response.ToolCalls
								);

								System.Console.WriteLine($"[DEBUG] Tool '{toolName}' executed successfully, result: {result}");

								toolResults.Add(new
								{
									tool_name = toolName,
									result = result
								});
							}
							catch (Exception ex)
							{
								System.Console.WriteLine($"[DEBUG] Tool '{toolName}' execution FAILED: {ex.GetType().Name}: {ex.Message}");
								// Add error result so Claude knows the tool failed
								toolResults.Add(new
								{
									tool_name = toolName,
									error = ex.Message
								});
							}
						}

						// Prepare next iteration input with tool results
						// Include original question so Claude has context
						currentInput = new Dictionary<string, object>
						{
							["tool_results"] = toolResults
						};

						if (!string.IsNullOrEmpty(originalQuestion))
						{
							currentInput["original_question"] = originalQuestion;
						}
					}
					else
					{
						// No tool calls - agent is done
						response.Output = functionResponse.Message ??
							functionResponse.Output?["output"]?.ToString() ??
							string.Empty;
						response.Success = true;
						break;
					}
				}

				// Update span with success
				if (_options.EnableTracing && currentSpanId != null)
				{
					await _client.Spans.UpdateAsync(
						currentSpanId,
						output: new Dictionary<string, object>
						{
							["output"] = response.Output,
							["iterations"] = response.Iterations
						},
						status: response.Success ? "completed" : "max_iterations_reached",
						cancellationToken: cancellationToken
					);
				}

				if (!response.Success && iteration >= _options.MaxIterations)
				{
					response.Error = $"Agent reached maximum iterations ({_options.MaxIterations}) without completing";
				}
			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Error = ex.Message;

				// Update span with error
				if (_options.EnableTracing && currentSpanId != null)
				{
					await _client.Spans.UpdateAsync(
						currentSpanId,
						status: "error",
						error: ex.Message,
						cancellationToken: cancellationToken
					);
				}
			}

			return response;
		}

		/// <summary>
		/// Runs the agent with a simple string input.
		/// </summary>
		public async Task<AgentResponse> RunAsync(
			string input,
			CancellationToken cancellationToken = default)
		{
			return await RunAsync(
				new Dictionary<string, object> { ["input"] = input },
				cancellationToken
			);
		}

		/// <summary>
		/// Adds a tool to the agent.
		/// </summary>
		public Agent WithTool(AgentTool tool)
		{
			_options.Tools.Add(tool);
			return this;
		}

		/// <summary>
		/// Adds multiple tools to the agent.
		/// </summary>
		public Agent WithTools(params AgentTool[] tools)
		{
			_options.Tools.AddRange(tools);
			return this;
		}

		/// <summary>
		/// Discovers and adds tools from an object's methods marked with [Tool] attribute.
		/// </summary>
		public Agent WithTools(object toolsProvider)
		{
			var tools = AgentTool.DiscoverTools(toolsProvider);
			_options.Tools.AddRange(tools);
			return this;
		}

		private async Task<object?> ExecuteToolAsync(
			string toolName,
			Dictionary<string, object?> arguments,
			List<ToolCall> toolCallHistory)
		{
			// Try exact match first, then case-insensitive
			var tool = _options.Tools.FirstOrDefault(t => t.Name == toolName)
				?? _options.Tools.FirstOrDefault(t => string.Equals(t.Name, toolName, StringComparison.OrdinalIgnoreCase));

			if (tool == null)
			{
				var availableTools = string.Join(", ", _options.Tools.Select(t => t.Name));
				System.Console.WriteLine($"[DEBUG] Tool '{toolName}' not found. Available tools: {availableTools}");
				throw new InvalidOperationException($"Tool '{toolName}' not found. Available: {availableTools}");
			}

			System.Console.WriteLine($"[DEBUG] Executing tool '{tool.Name}' with {arguments.Count} arguments");

			var toolCall = new ToolCall
			{
				ToolName = toolName,
				Arguments = arguments,
				Timestamp = DateTime.UtcNow
			};

			var stopwatch = Stopwatch.StartNew();
			try
			{
				if (tool.ExecuteAsync == null)
				{
					throw new InvalidOperationException($"Tool '{toolName}' has no execution handler");
				}

				var result = await tool.ExecuteAsync(arguments);
				toolCall.Result = result;
				stopwatch.Stop();
				toolCall.DurationMs = stopwatch.ElapsedMilliseconds;
				toolCallHistory.Add(toolCall);
				return result;
			}
			catch (Exception ex)
			{
				stopwatch.Stop();
				toolCall.DurationMs = stopwatch.ElapsedMilliseconds;
				toolCall.Error = ex.Message;
				toolCallHistory.Add(toolCall);
				throw;
			}
		}

		/// <summary>
		/// Converts AgentTools to the format expected by the Opper API.
		/// </summary>
		private static List<object>? ConvertToolsToApiFormat(List<AgentTool> tools)
		{
			if (tools == null || tools.Count == 0)
				return null;

			var apiTools = new List<object>();
			foreach (var tool in tools)
			{
				// Try OpenAI/Anthropic standard format first
				apiTools.Add(new
				{
					type = "function",
					function = new
					{
						name = tool.Name,
						description = tool.Description,
						parameters = tool.ParametersSchema
					}
				});
			}

			return apiTools;
		}

		/// <summary>
		/// Parses tool calls from Claude's native XML format in the message field.
		/// Supports formats like:
		/// - <function_calls><invoke name="ToolName"><arg name="param">value</arg></invoke></function_calls>
		/// - <tool_call>{"name": "ToolName", "arguments": {"param": "value"}}</tool_call>
		/// </summary>
		private static List<Dictionary<string, object>> ParseToolCallsFromMessage(string message)
		{
			var toolCalls = new List<Dictionary<string, object>>();

			// Try parsing <function_calls> XML format first
			var functionCallsMatch = System.Text.RegularExpressions.Regex.Match(
				message,
				@"<function_calls>(.*?)</function_calls>",
				System.Text.RegularExpressions.RegexOptions.Singleline
			);

			if (functionCallsMatch.Success)
			{
				var innerXml = functionCallsMatch.Groups[1].Value;

				// Find all <invoke> tags
				var invokeMatches = System.Text.RegularExpressions.Regex.Matches(
					innerXml,
					@"<invoke\s+name=""([^""]+)""[^>]*>(.*?)</invoke>",
					System.Text.RegularExpressions.RegexOptions.Singleline
				);

				foreach (System.Text.RegularExpressions.Match invokeMatch in invokeMatches)
				{
					var toolName = invokeMatch.Groups[1].Value;
					var argsXml = invokeMatch.Groups[2].Value;

					// Parse arguments from <arg> or <parameter> tags
					var arguments = new Dictionary<string, object>();
					var argMatches = System.Text.RegularExpressions.Regex.Matches(
						argsXml,
						@"<(?:arg|parameter)\s+name=""([^""]+)""[^>]*>([^<]*)</(?:arg|parameter)>",
						System.Text.RegularExpressions.RegexOptions.Singleline
					);

					foreach (System.Text.RegularExpressions.Match argMatch in argMatches)
					{
						var paramName = argMatch.Groups[1].Value;
						var paramValue = argMatch.Groups[2].Value.Trim();

						// Try to parse as number
						if (double.TryParse(paramValue, out var numValue))
						{
							arguments[paramName] = numValue;
						}
						else if (bool.TryParse(paramValue, out var boolValue))
						{
							arguments[paramName] = boolValue;
						}
						else
						{
							arguments[paramName] = paramValue;
						}
					}

					toolCalls.Add(new Dictionary<string, object>
					{
						["name"] = toolName,
						["arguments"] = arguments
					});
				}
			}

			// Try parsing <tool_call> JSON format if no function_calls found
			if (toolCalls.Count == 0)
			{
				// Find all <tool_call> tags and extract JSON properly
				var startIndex = 0;
				while ((startIndex = message.IndexOf("<tool_call>", startIndex)) != -1)
				{
					var contentStart = startIndex + "<tool_call>".Length;
					var endIndex = message.IndexOf("</tool_call>", contentStart);
					if (endIndex == -1) break;

					var jsonCandidate = ExtractJsonObject(message, contentStart);
					if (jsonCandidate != null)
					{
						try
						{
							var parsed = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonCandidate);
							if (parsed != null && TryNormalizeToolCall(parsed, out var normalized))
							{
								toolCalls.Add(normalized);
							}
						}
						catch
						{
							// Skip invalid JSON
						}
					}

					startIndex = endIndex + "</tool_call>".Length;
				}
			}

			// Try parsing inline JSON format: {"tool": "ToolName", ...}
			if (toolCalls.Count == 0)
			{
				// Find JSON objects that contain "tool", "tool_name", or "name" keys
				for (int i = 0; i < message.Length; i++)
				{
					if (message[i] == '{')
					{
						var jsonCandidate = ExtractJsonObject(message, i);
						if (jsonCandidate != null)
						{
							try
							{
								var parsed = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonCandidate);
								if (parsed != null && TryNormalizeToolCall(parsed, out var normalized))
								{
									toolCalls.Add(normalized);
									i += jsonCandidate.Length - 1; // Skip past this JSON
								}
							}
							catch
							{
								// Not valid JSON, continue
							}
						}
					}
				}
			}

			// Try parsing <tool>ToolName(args)</tool> format
			if (toolCalls.Count == 0)
			{
				var simpleToolMatches = System.Text.RegularExpressions.Regex.Matches(
					message,
					@"<tool>\s*(\w+)\s*\(([^)]*)\)\s*</tool>",
					System.Text.RegularExpressions.RegexOptions.Singleline
				);

				foreach (System.Text.RegularExpressions.Match match in simpleToolMatches)
				{
					var toolName = match.Groups[1].Value;
					var argsString = match.Groups[2].Value;

					var arguments = new Dictionary<string, object>();

					// Try to parse arguments as comma-separated values
					var argParts = argsString.Split(',');
					for (int i = 0; i < argParts.Length; i++)
					{
						var argValue = argParts[i].Trim();
						if (double.TryParse(argValue, out var numValue))
						{
							// Use generic parameter names (a, b, c, etc.)
							var paramName = i == 0 ? "a" : i == 1 ? "b" : $"arg{i}";
							arguments[paramName] = numValue;
						}
					}

					if (arguments.Count > 0)
					{
						toolCalls.Add(new Dictionary<string, object>
						{
							["name"] = toolName,
							["arguments"] = arguments
						});
					}
				}
			}

			return toolCalls;
		}

		/// <summary>
		/// Extracts a complete JSON object starting at the given position, handling nested braces.
		/// </summary>
		private static string? ExtractJsonObject(string text, int startIndex)
		{
			if (startIndex >= text.Length || text[startIndex] != '{')
				return null;

			int depth = 0;
			bool inString = false;
			bool escaped = false;

			for (int i = startIndex; i < text.Length; i++)
			{
				char c = text[i];

				if (escaped)
				{
					escaped = false;
					continue;
				}

				if (c == '\\' && inString)
				{
					escaped = true;
					continue;
				}

				if (c == '"')
				{
					inString = !inString;
					continue;
				}

				if (!inString)
				{
					if (c == '{')
					{
						depth++;
					}
					else if (c == '}')
					{
						depth--;
						if (depth == 0)
						{
							// Found the closing brace
							return text.Substring(startIndex, i - startIndex + 1);
						}
					}
				}
			}

			return null; // Unclosed JSON object
		}

		/// <summary>
		/// Tries to normalize a tool call dictionary into standard {name, arguments} format.
		/// </summary>
		private static bool TryNormalizeToolCall(Dictionary<string, object> parsed, out Dictionary<string, object> normalized)
		{
			normalized = new Dictionary<string, object>();

			// Extract tool name
			string? toolName = null;
			if (parsed.TryGetValue("name", out var nameVal))
			{
				toolName = nameVal?.ToString();
			}
			else if (parsed.TryGetValue("tool", out var toolVal))
			{
				toolName = toolVal?.ToString();
			}
			else if (parsed.TryGetValue("tool_name", out var toolNameVal))
			{
				toolName = toolNameVal?.ToString();
			}

			if (string.IsNullOrEmpty(toolName))
				return false;

			normalized["name"] = toolName;

			// Extract arguments
			Dictionary<string, object>? arguments = null;

			if (parsed.TryGetValue("arguments", out var argsVal))
			{
				if (argsVal is JObject argsObj)
				{
					arguments = argsObj.ToObject<Dictionary<string, object>>();
				}
				else if (argsVal is Dictionary<string, object> argsDict)
				{
					arguments = argsDict;
				}
			}
			else if (parsed.TryGetValue("parameters", out var paramsVal))
			{
				if (paramsVal is JObject paramsObj)
				{
					arguments = paramsObj.ToObject<Dictionary<string, object>>();
				}
				else if (paramsVal is Dictionary<string, object> paramsDict)
				{
					arguments = paramsDict;
				}
			}
			else if (parsed.TryGetValue("args", out var args2Val))
			{
				if (args2Val is JObject args2Obj)
				{
					arguments = args2Obj.ToObject<Dictionary<string, object>>();
				}
				else if (args2Val is Dictionary<string, object> args2Dict)
				{
					arguments = args2Dict;
				}
			}
			else
			{
				// Use all remaining keys as arguments
				arguments = new Dictionary<string, object>();
				foreach (var kvp in parsed)
				{
					if (kvp.Key != "name" && kvp.Key != "tool" && kvp.Key != "tool_name")
					{
						arguments[kvp.Key] = kvp.Value;
					}
				}
			}

			normalized["arguments"] = arguments ?? new Dictionary<string, object>();
			return true;
		}
	}
}
