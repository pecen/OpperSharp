using OpperSharp.Core;
using OpperSharp.Models.Functions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

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

			if (string.IsNullOrWhiteSpace(_options.FunctionPath))
				throw new ArgumentException("Function path is required", nameof(options));
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
					var span = await _client.Spans.CreateAsync(
						$"agent:{_options.FunctionPath}",
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
				var iteration = 0;

				while (iteration < _options.MaxIterations)
				{
					iteration++;
					response.Iterations = iteration;

					// Call the function with current input
					var callOptions = new OpperCallOptions
					{
						Context = _options.Context,
						ParentSpanId = currentSpanId,
						Model = _options.Model,
						Temperature = _options.Temperature,
						Metadata = _options.Metadata,
						Tools = ConvertToolsToApiFormat(_options.Tools)
					};

					// DEBUG: Log what's being sent
					System.Console.WriteLine($"[DEBUG] Iteration {iteration}: Calling {_options.FunctionPath}");
					System.Console.WriteLine($"[DEBUG] Tools count: {callOptions.Tools?.Count ?? 0}");
					if (callOptions.Tools != null && callOptions.Tools.Count > 0)
					{
						System.Console.WriteLine($"[DEBUG] First tool: {Newtonsoft.Json.JsonConvert.SerializeObject(callOptions.Tools[0])}");
					}

					var functionResponse = await _client.Functions.CallAsync(
						_options.FunctionPath,
						currentInput,
						callOptions,
						cancellationToken
					);

					// DEBUG: Log response
					System.Console.WriteLine($"[DEBUG] Response has tool_calls: {functionResponse.Output.ContainsKey("tool_calls")}");
					System.Console.WriteLine($"[DEBUG] Response message: {functionResponse.Message?.Substring(0, Math.Min(100, functionResponse.Message?.Length ?? 0))}");

					// Check if the response indicates tool calls
					if (functionResponse.Output.TryGetValue("tool_calls", out var toolCallsToken)
						&& toolCallsToken is JArray toolCallsArray
						&& toolCallsArray.Count > 0)
					{
						// Execute tool calls
						var toolResults = new List<object>();

						foreach (var toolCallToken in toolCallsArray)
						{
							var toolCall = toolCallToken.ToObject<Dictionary<string, object>>();
							if (toolCall == null) continue;

							var toolName = toolCall.GetValueOrDefault("name")?.ToString();
							var toolArgs = toolCall.GetValueOrDefault("arguments") as JObject;

							if (string.IsNullOrEmpty(toolName)) continue;

							var result = await ExecuteToolAsync(
								toolName,
								toolArgs?.ToObject<Dictionary<string, object?>>() ?? new Dictionary<string, object?>(),
								response.ToolCalls
							);

							toolResults.Add(new
							{
								tool_name = toolName,
								result = result
							});
						}

						// Prepare next iteration input with tool results
						currentInput = new Dictionary<string, object>
						{
							["tool_results"] = toolResults
						};
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
			var tool = _options.Tools.FirstOrDefault(t => t.Name == toolName);
			if (tool == null)
			{
				throw new InvalidOperationException($"Tool '{toolName}' not found");
			}

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
	}
}
