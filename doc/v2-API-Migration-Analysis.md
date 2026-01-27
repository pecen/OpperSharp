# OpperSharp v2 API Migration Analysis

**Generated**: 2026-01-27
**Status**: CRITICAL ISSUES IDENTIFIED

## Executive Summary

After comprehensive research of Opper AI's v2 API, several critical issues have been identified with OpperSharp's current implementation. The most critical finding is that **v2 uses `/knowledge` endpoints, NOT `/indexes`** - this is a breaking change that requires immediate attention.

## Critical Issues

### 🚨 Issue #1: Indexes → Knowledge (BREAKING CHANGE)

**Problem**: OpperSharp currently uses `/indexes` endpoints which appear to be from v1 API.

**v2 API Reality**: Uses `/knowledge` for knowledge base operations.

**Impact**:
- Current `IndexesClient` may not work with v2 API
- All index-related code needs to be updated to use `/knowledge` endpoints
- Method names and terminology need to change

**Required Changes**:
```csharp
// CURRENT (WRONG for v2)
/indexes                                    → Need to change to /knowledge
/indexes/{name}                             → /knowledge/{knowledge_base_id}
/indexes/{name}/index                       → /knowledge/{knowledge_base_id}/upload
/indexes/{name}/index/bulk                  → /knowledge/{knowledge_base_id}/upload (multi-file)
/indexes/{name}/query                       → Different approach in v2
/indexes/{name}/documents/{documentId}      → /knowledge/{knowledge_base_id}/files/{file_id}
```

### ⚠️ Issue #2: Missing v2 Endpoints

OpperSharp is missing several v2-only endpoints:

1. **Datasets** (`/datasets`) - For managing training data
2. **Embeddings** (`/embeddings`) - For generating vector embeddings
3. **Models Management** (`/models/*`) - For custom models and aliases
4. **OCR** (`/ocr`) - For document processing
5. **Rerank** (`/rerank`) - For search result optimization
6. **Analytics** (`/analytics/usage`) - For usage tracking
7. **OpenAI Compatibility** (`/openai/chat/completions`) - Drop-in replacement

## Detailed Endpoint Comparison

### ✅ Correctly Implemented Endpoints

| OpperSharp Endpoint | v2 API Endpoint | Status |
|---------------------|-----------------|--------|
| `/chat/completions` | `/chat/completions` | ✅ Correct |
| `/functions` | `/functions` | ✅ Correct |
| `/call` | `/call` | ✅ Correct |
| `/spans` | `/spans` | ✅ Correct |
| `/traces` | `/traces` | ✅ Correct |

### ❌ Incorrectly Implemented Endpoints

| OpperSharp Endpoint | Should Be | Impact |
|---------------------|-----------|---------|
| `/indexes` | `/knowledge` | 🚨 CRITICAL - Won't work in v2 |
| `/indexes/{name}` | `/knowledge/{knowledge_base_id}` | 🚨 CRITICAL |
| `/indexes/{name}/index` | `/knowledge/{knowledge_base_id}/upload` | 🚨 CRITICAL |
| `/indexes/{name}/query` | Different v2 approach | 🚨 CRITICAL |

## v2 API Structure

### Base URL
```
https://api.opper.ai/v2
```

### Complete v2 Endpoint List

#### 1. Core Call Endpoints
- `POST /call` - Execute LLM task
- `POST /call/stream` - Stream LLM task execution

#### 2. Functions Endpoints
- `POST /functions` - Create function
- `GET /functions` - List functions (paginated)
- `GET /functions/{function_id}` - Get function
- `PATCH /functions/{function_id}` - Update function
- `DELETE /functions/{function_id}` - Delete function
- `GET /functions/by-name/{name}` - Get by name
- `POST /functions/{function_id}/call` - Call saved function
- `POST /functions/{function_id}/call/stream` - Stream saved function

#### 3. Knowledge Base Endpoints (NOT INDEXES!)
- `POST /knowledge` - Create knowledge base
- `GET /knowledge` - List knowledge bases
- `GET /knowledge/{knowledge_base_id}` - Get knowledge base
- `DELETE /knowledge/{knowledge_base_id}` - Delete knowledge base
- `GET /knowledge/by-name/{knowledge_base_name}` - Get by name
- `GET /knowledge/{knowledge_base_id}/upload_url` - Get presigned upload URL
- `POST /knowledge/{knowledge_base_id}/register_file` - Register file upload
- `POST /knowledge/{knowledge_base_id}/upload` - Upload file directly
- `GET /knowledge/{knowledge_base_id}/files` - List files
- `DELETE /knowledge/{knowledge_base_id}/files/{file_id}` - Delete file
- `GET /knowledge/{knowledge_base_id}/files/{file_id}/download_url` - Get download URL

#### 4. Datasets Endpoints (MISSING in OpperSharp)
- `POST /datasets/{dataset_id}` - Create entry
- `GET /datasets/{dataset_id}/entries` - List entries
- `GET /datasets/{dataset_id}/entries/{entry_id}` - Get entry
- `DELETE /datasets/{dataset_id}/entries/{entry_id}` - Delete entry
- `POST /datasets/{dataset_id}/entries/query` - Query entries

#### 5. Spans & Traces Endpoints
- `POST /spans` - Create span
- `GET /spans/{span_id}` - Get span
- `PATCH /spans/{span_id}` - Update span
- `DELETE /spans/{span_id}` - Delete span
- `POST /spans/{span_id}/save_examples` - Save to dataset
- `POST /spans/{span_id}/feedback` - Submit feedback
- `GET /traces` - List traces
- `GET /traces/{trace_id}` - Get trace

#### 6. Models & Aliases Endpoints (MISSING in OpperSharp)
- `GET /models` - List models
- `POST /models/custom` - Register custom model
- `GET /models/custom` - List custom models
- `GET /models/custom/{model_id}` - Get custom model
- `PATCH /models/custom/{model_id}` - Update custom model
- `DELETE /models/custom/{model_id}` - Delete custom model
- `POST /models/aliases` - Create alias
- `GET /models/aliases` - List aliases
- `GET /models/aliases/{alias_id}` - Get alias
- `PATCH /models/aliases/{alias_id}` - Update alias
- `DELETE /models/aliases/{alias_id}` - Delete alias

#### 7. Embeddings Endpoint (MISSING in OpperSharp)
- `POST /embeddings` - Generate embeddings

#### 8. OCR Endpoints (MISSING in OpperSharp)
- `POST /ocr` - Process OCR
- `GET /ocr/models` - List OCR models

#### 9. Rerank Endpoints (MISSING in OpperSharp)
- `POST /rerank` - Rerank documents
- `GET /rerank/models` - List rerank models

#### 10. Analytics Endpoint (MISSING in OpperSharp)
- `GET /analytics/usage` - Get usage analytics

## Required Changes

### Priority 1: Critical Fixes (Breaking Changes)

1. **Rename `IndexesClient` to `KnowledgeClient`**
   - Update all `/indexes` references to `/knowledge`
   - Change method names to reflect "knowledge base" terminology
   - Update models to match v2 structure

2. **Update EndPoints Enum**
   ```csharp
   // CURRENT
   [Description("/indexes")]
   Indexes,

   // SHOULD BE
   [Description("/knowledge")]
   Knowledge,
   ```

3. **Update IndexesClient Methods**
   - Change from document-based to file-based operations
   - Add support for presigned URLs
   - Add file management operations
   - Remove document query (different approach in v2)

### Priority 2: Add Missing v2 Endpoints

1. **Create `DatasetsClient`** - For training data management
2. **Create `EmbeddingsClient`** - For vector embeddings
3. **Create `ModelsClient`** - For model and alias management
4. **Create `OcrClient`** - For OCR processing
5. **Create `RerankClient`** - For document reranking
6. **Create `AnalyticsClient`** - For usage analytics

### Priority 3: Update Documentation

All documentation files created in this session need to be updated:

1. `OpperSharp-Presentation-Swedish-Professional.html`
2. `OpperSharp-Presentation-Swedish-Notes.html`
3. `OpperSharp-Presentation-Swedish.html`
4. `OpperSharp-Presentation-Natural.html`
5. `OpperSharp-Complete-Overview.md`
6. `SDK-Usage-Examples.md`
7. `QuickStart-Guide.md`
8. `README.md`

**Changes Required**:
- Replace all references to "Indexes" with "Knowledge Bases"
- Update code examples to use new `/knowledge` endpoints
- Add documentation for new v2 features
- Update architecture diagrams if any

## v2 Knowledge Base API Structure

### Create Knowledge Base
```csharp
POST /knowledge
{
  "name": "string (required)",
  "embedding_model": "string (optional, default: azure/text-embedding-3-large)"
}
```

### Upload File to Knowledge Base
```csharp
// Option 1: Get presigned URL
GET /knowledge/{knowledge_base_id}/upload_url

// Option 2: Direct upload
POST /knowledge/{knowledge_base_id}/upload
{
  "filename": "string",
  "file_id": "string",
  "content_type": "string",
  "configuration": {},
  "metadata": {}
}

// Option 3: Register file (for already uploaded files)
POST /knowledge/{knowledge_base_id}/register_file
{
  "filename": "string",
  "file_id": "string",
  "content_type": "string",
  "configuration": {},
  "metadata": {}
}
```

### List Files
```csharp
GET /knowledge/{knowledge_base_id}/files
// Supports pagination: offset (default: 0), limit (default: 100)
```

### Delete File
```csharp
DELETE /knowledge/{knowledge_base_id}/files/{file_id}
```

## Implementation Recommendations

### Phase 1: Critical Fixes (Immediate)
1. Update EndPoints enum to use `/knowledge`
2. Rename IndexesClient to KnowledgeClient
3. Update all knowledge-related methods and models
4. Test knowledge base operations

### Phase 2: Add Core Missing Features (High Priority)
1. Implement DatasetsClient
2. Implement EmbeddingsClient
3. Implement ModelsClient (with alias support)
4. Update documentation

### Phase 3: Add Advanced Features (Medium Priority)
1. Implement OcrClient
2. Implement RerankClient
3. Implement AnalyticsClient
4. Add comprehensive examples

### Phase 4: Polish (Low Priority)
1. Add OpenAI compatibility mode
2. Add advanced retry strategies
3. Performance optimizations
4. Comprehensive integration tests

## Breaking Changes for Users

If OpperSharp users exist, they will need to:

1. **Update Code References**:
   ```csharp
   // OLD
   await client.Indexes.CreateAsync("my-index");
   await client.Indexes.IndexAsync("my-index", content);
   await client.Indexes.QueryAsync("my-index", query);

   // NEW
   await client.Knowledge.CreateAsync("my-knowledge-base");
   await client.Knowledge.UploadFileAsync("my-knowledge-base", file);
   // Query is done differently in v2 (integrated with functions)
   ```

2. **Update Terminology**: All references to "indexes" need to become "knowledge bases"

3. **Update Query Approach**: v2 integrates knowledge bases with function calls rather than separate query operations

## Key v2 Architectural Changes

### 1. Knowledge Bases are File-Based
- v1: Document-based indexing
- v2: File-based knowledge bases (PDF, CSV, TXT)
- Impact: Different upload/management paradigm

### 2. Integrated RAG
- v1: Separate index query + function call
- v2: Functions automatically use knowledge bases via configuration
- Impact: Simpler RAG implementation

### 3. Model Aliases & Fallbacks
- v1: No built-in fallback support
- v2: Model aliases with automatic failover
- Impact: Better reliability and cost optimization

### 4. Enhanced Observability
- v1: Basic spans
- v2: Comprehensive spans with feedback, datasets, and analytics
- Impact: Better debugging and evaluation

## Next Steps

1. **Confirm v2 API Details**: Verify if `/indexes` still exists in v2 or is completely replaced
2. **Update EndPoints Enum**: Add `/knowledge` and other missing endpoints
3. **Create/Update Clients**: KnowledgeClient, DatasetsClient, etc.
4. **Update Models**: Create new model classes for v2 structures
5. **Update Documentation**: All presentation and guide files
6. **Testing**: Comprehensive testing against v2 API
7. **Migration Guide**: Create guide for users migrating from v1-based code

## References

- [Opper v2 API Documentation](https://docs.opper.ai)
- [Opper Node SDK (v2)](https://github.com/opper-ai/opper-node)
- [Opper Python SDK (v2)](https://github.com/opper-ai/opper-python)
- [Knowledge Bases Documentation](https://docs.opper.ai/capabilities/indexes)
- [OpenAI Compatible API](https://opper.ai/blog/openai-compatible-api)

---

**This analysis provides a complete roadmap for migrating OpperSharp to v2 API compatibility.**
