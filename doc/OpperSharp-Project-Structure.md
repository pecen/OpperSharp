# OpperSharp Project Structure

A comprehensive overview of the OpperSharp SDK architecture and file organization.

## Overview

OpperSharp is a .NET 8.0 SDK for the Opper AI v2 API, organized into multiple projects with clear separation of concerns:

- **Core**: Main client orchestrator
- **Clients**: API endpoint implementations for each service
- **Agents**: Agent framework for multi-step AI workflows with tool support
- **Models**: Data models organized by feature area
- **Utilities**: Configuration and infrastructure
- **Exceptions**: Typed error handling
- **UI**: Test console application

---

## Project Structure

```
OpperSharp/
├── OpperSharp.Core/
│   └── OpperClient.cs                              # Main SDK entry point, orchestrates all clients
│
├── OpperSharp.Clients/
│   ├── FunctionsClient.cs                          # Function operations (create, call, delete)
│   ├── IndexesClient.cs                            # Vector/semantic search operations
│   ├── SpansClient.cs                              # Distributed tracing and observability
│   ├── KnowledgeClient.cs                          # Knowledge base management (RAG)
│   ├── EmbeddingsClient.cs                         # Vector embedding generation
│   ├── ModelsClient.cs                             # Model listing and alias management
│   ├── AnalyticsClient.cs                          # Analytics and usage metrics
│   ├── DatasetsClient.cs                           # Dataset management for training/evaluation
│   ├── OcrClient.cs                                # Optical character recognition
│   └── RerankClient.cs                             # Document reranking for search
│
├── OpperSharp.Agents/
│   ├── Agent.cs                                    # Agent orchestrator with multi-step execution
│   ├── AgentTool.cs                                # Tool abstraction and discovery
│   ├── AgentOptions.cs                             # Agent configuration (includes AgentProgressUpdate)
│   ├── AgentResponse.cs                            # Agent execution result
│   ├── ToolAttribute.cs                            # [Tool] decorator for automatic discovery
│   └── ToolCall.cs                                 # Tool execution record
│
├── OpperSharp.Models.Functions/
│   ├── OpperFunction.cs                            # Function metadata
│   ├── OpperFunctionDefinition.cs                  # Function create/update payload
│   ├── OpperFunctionResponse.cs                    # Function call result
│   └── OpperCallOptions.cs                         # Function call configuration
│
├── OpperSharp.Models.Chat/
│   ├── OpperChatChoice.cs                          # Single chat completion choice
│   ├── OpperChatResponse.cs                        # Complete chat API response
│   ├── OpperChatStreamChoice.cs                    # Single choice in stream
│   ├── OpperChatStreamChunk.cs                     # Streamed response chunk
│   ├── OpperMessage.cs                             # Chat message (user/assistant/system)
│   └── OpperUsage.cs                               # Token usage statistics
│
├── OpperSharp.Models.Indexes/
│   ├── OpperIndex.cs                               # Index metadata
│   ├── OpperDocument.cs                            # Document input for indexing
│   ├── OpperIndexedDocument.cs                     # Document with assigned ID
│   └── OpperSearchResponse.cs                      # Semantic search results
│
├── OpperSharp.Models.Knowledge/
│   ├── OpperKnowledgeBase.cs                       # Knowledge base metadata
│   ├── OpperKnowledgeFile.cs                       # File metadata in knowledge base
│   ├── OpperFileUploadRequest.cs                   # File upload payload
│   └── OpperPresignedUrl.cs                        # Presigned URL for uploads
│
├── OpperSharp.Models.Embeddings/
│   ├── OpperEmbeddingRequest.cs                    # Embedding generation request
│   └── OpperEmbeddingResponse.cs                   # Vector embedding result
│
├── OpperSharp.Models.Models/
│   ├── OpperModel.cs                               # Model metadata (name, provider)
│   └── OpperModelAlias.cs                          # Model alias with fallback chain
│
├── OpperSharp.Models.Analytics/
│   └── OpperAnalyticsResponse.cs                   # Analytics query results
│
├── OpperSharp.Models.Datasets/
│   ├── OpperDatasetEntry.cs                        # Single dataset entry
│   └── OpperDatasetQueryRequest.cs                 # Dataset query parameters
│
├── OpperSharp.Models.Ocr/
│   └── OpperOcrRequest.cs                          # OCR processing request
│
├── OpperSharp.Models.Rerank/
│   └── OpperRerankRequest.cs                       # Document reranking request
│
├── OpperSharp.Models.Spans/
│   └── OpperSpan.cs                                # Distributed tracing span
│
├── OpperSharp.Models.Common/
│   ├── OpperStreamChunk.cs                         # Generic streaming chunk
│   └── OpperListResponse.cs                        # Generic paginated list response
│
├── OpperSharp.Utilities/
│   ├── OpperClientOptions.cs                       # SDK configuration options
│   └── RetryHandler.cs                             # Exponential backoff retry logic
│
├── OpperSharp.Exceptions/
│   └── OpperAPIException.cs                        # Typed API error with status codes
│
└── OpperSharp.UI.TestConsole/
    ├── Program.cs                                  # Test console with 12 menu items
    ├── README.md                                   # English documentation
    └── README.sv.md                                # Swedish documentation
```

---

## Key Components

### Core

**OpperClient.cs**
- Main SDK entry point
- Initializes all specialized clients
- Manages HTTP client lifecycle
- Provides unified API surface

### Clients

All clients follow a consistent pattern:
- Constructor accepts `HttpClient` and `OpperClientOptions`
- Async methods for all operations
- Proper error handling with `OpperAPIException`
- Support for retry logic via `RetryHandler`

**FunctionsClient**: Create, call, and manage AI functions (ad-hoc and named)
**IndexesClient**: Vector search and semantic indexing
**SpansClient**: Distributed tracing for observability
**KnowledgeClient**: RAG with file uploads and knowledge bases
**EmbeddingsClient**: Generate vector embeddings for semantic similarity
**ModelsClient**: List models and manage aliases with fallback chains
**AnalyticsClient**: Query usage metrics and analytics
**DatasetsClient**: Manage training/evaluation datasets
**OcrClient**: Extract text from images
**RerankClient**: Rerank search results for relevance

### Agents

The Agent framework enables multi-step AI workflows:

**Agent.cs**
- Executes multi-iteration workflows
- Calls tools based on AI decisions
- Handles context preservation across iterations
- Provides progress callbacks via `OnProgress`

**AgentTool.cs**
- Two creation patterns:
  - `DiscoverTools<T>()` - Finds methods with `[Tool]` attribute
  - `Create()` - Lambda-based tool definition
- Case-insensitive tool name matching

**AgentOptions.cs**
- Configuration: name, instructions, model, temperature
- `MaxIterations` controls execution length
- `OnProgress` callback for real-time feedback
- Includes `AgentProgressUpdate` class for progress reporting

**Key Features:**
- Automatic tool discovery via attributes
- JSON parameter serialization
- Error handling and retry logic
- Real-time progress updates showing iteration, tools called, and agent thoughts

### Models

Models are organized by feature area into separate namespaces:

- **Functions**: Function definitions and call options
- **Chat**: Chat completions and streaming
- **Indexes**: Vector search and document indexing
- **Knowledge**: Knowledge base and file management
- **Embeddings**: Vector embedding generation
- **Models**: Model metadata and aliases
- **Analytics**: Usage metrics
- **Datasets**: Training data management
- **Ocr**: Image text extraction
- **Rerank**: Search result reranking
- **Spans**: Distributed tracing
- **Common**: Shared types (streaming, pagination)

All models use `Newtonsoft.Json` for serialization with proper `[JsonProperty]` attributes.

### Utilities

**OpperClientOptions.cs**
- Base URL configuration
- API key management
- Timeout settings
- Custom headers

**RetryHandler.cs**
- Exponential backoff retry logic
- Configurable retry attempts
- Handles transient HTTP errors

### Exceptions

**OpperAPIException.cs**
- HTTP status code
- Error message from API
- Request context
- Used throughout SDK for consistent error handling

### UI - TestConsole

**Program.cs**
Comprehensive test application with 12 menu items:

**Agent Tests (1-3):**
- Basic Math: Math tools with `[Tool]` attribute
- Custom Tools: Lambda-based tool creation
- Multi-step: Complex salary calculations

**Basic API Tests (4-6):**
- Simple Function Call: Ad-hoc function calls
- Streaming Response: Story generation
- Conversational Chat: Multi-turn conversations

**v2 API Tests (7-9):**
- Knowledge Base: File upload and RAG
- Embeddings: Vector generation and similarity
- Model Aliases: Fallback chains

**Consultant Matching (10-12):**
- Basic Matching: 3 consultants with 2 tools
- Scale Test: 25 consultants with rich CV data
- Error Handling: Resilient agents with unreliable tools

**Debug Utilities (94-99):**
- Direct HTTP calls, function management, cleanup utilities

**README.md & README.sv.md**
- Complete documentation in English and Swedish
- Architecture examples
- Performance tips
- Troubleshooting guides

---

## Architecture Patterns

### Separation of Concerns
- **Core**: Orchestration only
- **Clients**: API operations, no business logic
- **Models**: Data structures only, no behavior
- **Agents**: Workflow orchestration

### Model Namespaces
Each feature area has its own model namespace to avoid naming conflicts and improve discoverability.

### Ad-hoc Function Calls
The SDK now uses ad-hoc calls by default (name, instructions, model in request body), making named functions in the dashboard optional.

### Agent Tool Patterns
Two patterns for maximum flexibility:
1. **Attribute-based**: `[Tool]` on methods for automatic discovery
2. **Lambda-based**: `AgentTool.Create()` for dynamic tools

### Real-time Feedback
`OnProgress` callbacks provide users with visibility into:
- Current iteration number
- Tools being invoked
- Agent's reasoning process
- Interactive feedback during long operations

### Error Resilience
- Retry logic with exponential backoff
- Typed exceptions with context
- Graceful handling of tool failures
- Temperature = 0.0 for deterministic behavior when needed

---

## Dependencies

- **.NET 8.0**: Target framework
- **Newtonsoft.Json**: JSON serialization
- **Microsoft.Extensions.Configuration**: User Secrets support
- **System.Text.Json**: JSON utilities (secondary)

---

## Testing

The TestConsole application (`OpperSharp.UI.TestConsole`) provides comprehensive testing:
- All 10 client types tested
- Agent framework with 6 different scenarios
- Real-world use case (consultant matching)
- Error handling and resilience testing
- Performance optimization examples

---

## Next Steps

1. **Production Integration**: Replace hard-coded data with real APIs (e.g., CINode)
2. **Enhanced Matching**: Add semantic similarity using embeddings
3. **More Tools**: Availability, history, location, certifications
4. **GUI**: Build user interface for consultant matching
5. **RAG Integration**: Use Knowledge Base for assignment history

---

**Last Updated**: 2026-02-06
**SDK Version**: v2
**Documentation**: See `/doc` folder and TestConsole README files
