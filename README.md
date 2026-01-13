# OpperSharp
A C# SDK that wraps the functionality in the Opper API. 
It has basically the same set of methods as Opper's own Python SDK and TypeScript SDK. The idea came from since Opper only supports Python and TypeScript in their SDKs, I wanted to write a C# equivalence since I work mostly in C#. So it's fair to say that OpperSharp mirrors the Opper Python SDK.

## OpperSharp capabilities 
OpperSharp has a fully functional C# SDK that mirrors the capabilities of Opper's Python SDK, including:

1.	Core API operations (Functions, Indexes, Chat, Spans)
2.	Agent framework with tool support
3.	Attribute-based tool discovery (like Python's @tool decorator)
4.	Fluent builder pattern for easy configuration
5.	Full async/await support
6.	Streaming capabilities
7.	Proper error handling
