using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace OpperSharp.Agents
{
	/// <summary>
	/// Represents a tool that an agent can use.
	/// </summary>
	public class AgentTool
	{
		/// <summary>
		/// Unique name of the tool.
		/// </summary>
		public string Name { get; set; } = string.Empty;

		/// <summary>
		/// Description of what the tool does.
		/// </summary>
		public string Description { get; set; } = string.Empty;

		/// <summary>
		/// JSON Schema for the tool's parameters.
		/// </summary>
		public JObject ParametersSchema { get; set; } = new();

		/// <summary>
		/// The delegate to execute when the tool is called.
		/// </summary>
		internal Func<Dictionary<string, object?>, Task<object?>>? ExecuteAsync { get; set; }

		/// <summary>
		/// Creates a tool from an async delegate with typed input.
		/// </summary>
		public static AgentTool Create<TInput, TOutput>(
			string name,
			string description,
			Func<TInput, Task<TOutput>> handler)
			where TInput : class
		{
			var tool = new AgentTool
			{
				Name = name,
				Description = description,
				ParametersSchema = GenerateSchema(typeof(TInput))
			};

			tool.ExecuteAsync = async (parameters) =>
			{
				var json = JsonConvert.SerializeObject(parameters);
				var input = JsonConvert.DeserializeObject<TInput>(json);
				if (input == null)
					throw new ArgumentException("Failed to deserialize input parameters");

				return await handler(input);
			};

			return tool;
		}

		/// <summary>
		/// Creates a tool from a synchronous delegate with typed input.
		/// </summary>
		public static AgentTool Create<TInput, TOutput>(
			string name,
			string description,
			Func<TInput, TOutput> handler)
			where TInput : class
		{
			return Create<TInput, TOutput>(
				name,
				description,
				input => Task.FromResult(handler(input))
			);
		}

		/// <summary>
		/// Creates a simple tool with string input and output.
		/// </summary>
		public static AgentTool Create(
			string name,
			string description,
			Func<string, Task<string>> handler)
		{
			var tool = new AgentTool
			{
				Name = name,
				Description = description,
				ParametersSchema = JObject.FromObject(new
				{
					type = "object",
					properties = new
					{
						input = new { type = "string", description = "Input to the tool" }
					},
					required = new[] { "input" }
				})
			};

			tool.ExecuteAsync = async (parameters) =>
			{
				var input = parameters.GetValueOrDefault("input")?.ToString() ?? string.Empty;
				return await handler(input);
			};

			return tool;
		}

		/// <summary>
		/// Creates a simple synchronous tool with string input and output.
		/// </summary>
		public static AgentTool Create(
			string name,
			string description,
			Func<string, string> handler)
		{
			return Create(name, description, input => Task.FromResult(handler(input)));
		}

		/// <summary>
		/// Creates a tool from a method with the [Tool] attribute.
		/// </summary>
		public static AgentTool FromMethod(MethodInfo method, object? instance = null)
		{
			var attr = method.GetCustomAttribute<ToolAttribute>();
			var name = attr?.Name ?? method.Name;
			var description = attr?.Description ?? $"Executes {method.Name}";

			var tool = new AgentTool
			{
				Name = name,
				Description = description,
				ParametersSchema = GenerateSchemaFromMethod(method)
			};

			tool.ExecuteAsync = async (parameters) =>
			{
				var methodParams = method.GetParameters();
				var args = new object?[methodParams.Length];

				for (int i = 0; i < methodParams.Length; i++)
				{
					var param = methodParams[i];
					if (parameters.TryGetValue(param.Name ?? $"arg{i}", out var value))
					{
						args[i] = ConvertParameter(value, param.ParameterType);
					}
					else if (param.HasDefaultValue)
					{
						args[i] = param.DefaultValue;
					}
					else
					{
						throw new ArgumentException($"Missing required parameter: {param.Name}");
					}
				}

				var result = method.Invoke(instance, args);

				if (result is Task task)
				{
					await task;
					var resultProperty = task.GetType().GetProperty("Result");
					return resultProperty?.GetValue(task);
				}

				return result;
			};

			return tool;
		}

		/// <summary>
		/// Discovers all tools from an object's methods marked with [Tool] attribute.
		/// </summary>
		public static List<AgentTool> DiscoverTools(object toolsProvider)
		{
			var tools = new List<AgentTool>();
			var type = toolsProvider.GetType();

			foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
			{
				if (method.GetCustomAttribute<ToolAttribute>() != null)
				{
					tools.Add(FromMethod(method, toolsProvider));
				}
			}

			return tools;
		}

		private static JObject GenerateSchema(Type type)
		{
			var properties = new Dictionary<string, object>();
			var required = new List<string>();

			foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				var propSchema = GetPropertySchema(prop.PropertyType);
				propSchema["description"] = prop.Name;
				properties[ToCamelCase(prop.Name)] = propSchema;

				if (!IsNullable(prop.PropertyType))
				{
					required.Add(ToCamelCase(prop.Name));
				}
			}

			return JObject.FromObject(new
			{
				type = "object",
				properties,
				required
			});
		}

		private static JObject GenerateSchemaFromMethod(MethodInfo method)
		{
			var properties = new Dictionary<string, object>();
			var required = new List<string>();

			foreach (var param in method.GetParameters())
			{
				var propSchema = GetPropertySchema(param.ParameterType);
				propSchema["description"] = param.Name ?? $"Parameter {param.Position}";
				properties[param.Name ?? $"arg{param.Position}"] = propSchema;

				if (!param.HasDefaultValue && !IsNullable(param.ParameterType))
				{
					required.Add(param.Name ?? $"arg{param.Position}");
				}
			}

			return JObject.FromObject(new
			{
				type = "object",
				properties,
				required
			});
		}

		private static Dictionary<string, object> GetPropertySchema(Type type)
		{
			var schema = new Dictionary<string, object>();
			var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

			if (underlyingType == typeof(string))
			{
				schema["type"] = "string";
			}
			else if (underlyingType == typeof(int) || underlyingType == typeof(long))
			{
				schema["type"] = "integer";
			}
			else if (underlyingType == typeof(double) || underlyingType == typeof(float) || underlyingType == typeof(decimal))
			{
				schema["type"] = "number";
			}
			else if (underlyingType == typeof(bool))
			{
				schema["type"] = "boolean";
			}
			else if (underlyingType == typeof(DateTime))
			{
				schema["type"] = "string";
				schema["format"] = "date-time";
			}
			else if (underlyingType.IsArray || (underlyingType.IsGenericType && underlyingType.GetGenericTypeDefinition() == typeof(List<>)))
			{
				schema["type"] = "array";
				var elementType = underlyingType.IsArray
					? underlyingType.GetElementType()!
					: underlyingType.GetGenericArguments()[0];
				schema["items"] = GetPropertySchema(elementType);
			}
			else
			{
				schema["type"] = "object";
			}

			return schema;
		}

		private static object? ConvertParameter(object? value, Type targetType)
		{
			if (value == null)
				return null;

			var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

			if (value is JToken jToken)
			{
				return jToken.ToObject(targetType);
			}

			if (underlyingType == typeof(string))
				return value.ToString();

			if (underlyingType == typeof(int))
				return Convert.ToInt32(value);

			if (underlyingType == typeof(long))
				return Convert.ToInt64(value);

			if (underlyingType == typeof(double))
				return Convert.ToDouble(value);

			if (underlyingType == typeof(float))
				return Convert.ToSingle(value);

			if (underlyingType == typeof(decimal))
				return Convert.ToDecimal(value);

			if (underlyingType == typeof(bool))
				return Convert.ToBoolean(value);

			if (underlyingType == typeof(DateTime))
				return DateTime.Parse(value.ToString() ?? string.Empty);

			var json = JsonConvert.SerializeObject(value);
			return JsonConvert.DeserializeObject(json, targetType);
		}

		private static bool IsNullable(Type type)
		{
			return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
		}

		private static string ToCamelCase(string str)
		{
			if (string.IsNullOrEmpty(str))
				return str;
			return char.ToLowerInvariant(str[0]) + str[1..];
		}
	}
}
