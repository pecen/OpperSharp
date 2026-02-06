# Documentation Updates Required for v2 API Migration

**Date**: 2026-01-27
**Status**: SDK Migration Complete - Documentation Updates Needed

## Summary of SDK Changes

### Phase 1: Critical Fixes
- **Indexes → Knowledge**: Replaced `/indexes` with `/knowledge` endpoints
- Created new `KnowledgeClient` with file-based operations
- Deprecated `IndexesClient` (kept for backwards compatibility)

### Phase 2: Core Features
- Added **Datasets** API - Training data management
- Added **Embeddings** API - Vector embedding generation
- Added **Models** API - Model and alias management with fallback support

### Phase 3: Advanced Features
- Added **OCR** API - Document processing
- Added **Rerank** API - Document relevance ranking
- Added **Analytics** API - Usage tracking and metrics

## Documentation Files Requiring Updates

### 1. README.md
**Location**: `/home/user/OpperSharp/README.md`

**Changes Needed**:
- Update API coverage section to include new v2 endpoints
- Replace "Indexes" references with "Knowledge Bases"
- Add new sections for Datasets, Embeddings, Models, OCR, Rerank, Analytics
- Update code examples to use `client.Knowledge` instead of `client.Indexes`
- Add deprecation notice for IndexesClient

**Key Points**:
- Emphasize v2 API compatibility
- Highlight new features (model aliases, OCR, rerank, etc.)
- Show file-based knowledge base examples

### 2. Presentation Files

**Files to Update**:
- `OpperSharp-Presentation-Swedish-Professional.html`
- `OpperSharp-Presentation-Swedish-Notes.html`
- `OpperSharp-Presentation-Swedish.html`
- `OpperSharp-Presentation-Natural.html`
- `OpperSharp-Presentation.html`
- `OpperSharp-Presentation.md`

**Changes Needed**:
- Replace all "Indexes" terminology with "Knowledge Bases"
- Update code examples to use Knowledge API (file-based)
- Add slides for new v2 features:
  - Datasets (training data)
  - Embeddings (vector generation)
  - Models & Aliases (fallback support)
  - OCR (document processing)
  - Rerank (search optimization)
  - Analytics (usage metrics)
- Update architecture diagrams if present
- Emphasize v2 API advantages

### 3. SDK-Usage-Examples.md
**Location**: `/home/user/OpperSharp/doc/SDK-Usage-Examples.md`

**Changes Needed**:
- Section 5: Replace Indexes examples with Knowledge examples
- Show file upload examples (presigned URLs)
- Add new sections:
  - **Datasets**: Creating training entries
  - **Embeddings**: Generating vectors
  - **Models**: Creating aliases with fallbacks
  - **OCR**: Processing documents
  - **Rerank**: Optimizing search results
  - **Analytics**: Querying usage data

### 4. QuickStart-Guide.md
**Location**: `/home/user/OpperSharp/doc/QuickStart-Guide.md`

**Changes Needed**:
- Example 2: Update knowledge base example to use file uploads
- Add new examples:
  - **Example 11**: Creating embeddings for custom search
  - **Example 12**: Setting up model aliases for reliability
  - **Example 13**: Processing documents with OCR
  - **Example 14**: Reranking search results
  - **Example 15**: Querying usage analytics

### 5. OpperSharp-Complete-Overview.md
**Location**: `/home/user/OpperSharp/doc/OpperSharp-Complete-Overview.md`

**Changes Needed**:
- Update "What is OpperSharp" section with v2 API mention
- Architecture section: Add new clients
- Python SDK comparison: Update with new v2 features
- Feature Analysis: Add sections for all new APIs
- Update "Capabilities" to reflect v2 completeness

## Key Messaging Changes

### Old Messaging (v1-based):
- "Indexes for vector storage and retrieval"
- "Document-based indexing"
- "Query indexes for semantic search"

### New Messaging (v2-based):
- "Knowledge Bases for file-based RAG"
- "Upload PDFs, CSVs, and text files"
- "Integrated with function calls for seamless RAG"
- "Model aliases for automatic fallback"
- "Complete v2 API coverage with advanced features"

## Code Example Updates

### OLD (v1):
```csharp
// Create index
await client.Indexes.CreateAsync("my-index");

// Add documents
await client.Indexes.IndexAsync("my-index", documentContent);

// Query
var results = await client.Indexes.QueryAsync("my-index", query);
```

### NEW (v2):
```csharp
// Create knowledge base
await client.Knowledge.CreateAsync("my-knowledge-base");

// Upload file
using var fileStream = File.OpenRead("document.pdf");
await client.Knowledge.UploadFileAsync(
    "my-knowledge-base",
    "document.pdf",
    fileStream,
    "application/pdf"
);

// Use with functions (integrated RAG)
var result = await client.Functions.CallAsync(
    "answer-question",
    new { question = "What is the policy?" },
    new OpperCallOptions
    {
        Context = new() { ["knowledge_base"] = "my-knowledge-base" }
    }
);
```

## New Feature Examples to Add

### 1. Embeddings
```csharp
var embeddings = await client.Embeddings.CreateAsync(
    "Text to embed",
    model: "azure/text-embedding-3-large"
);
```

### 2. Model Aliases (Fallback)
```csharp
await client.Models.CreateAliasAsync(
    "reliable-gpt4",
    new List<string> { "gpt-4", "gpt-4-turbo", "gpt-3.5-turbo" },
    "GPT-4 with fallbacks"
);
```

### 3. OCR
```csharp
var ocrResult = await client.Ocr.ProcessAsync(
    new OpperOcrRequest
    {
        Model = "gpt-4-vision",
        Document = documentData
    }
);
```

### 4. Rerank
```csharp
var reranked = await client.Rerank.RerankAsync(
    query: "best practices",
    documents: searchResults,
    model: "rerank-model",
    topK: 5
);
```

### 5. Datasets
```csharp
await client.Datasets.CreateEntryAsync(
    "training-data",
    input: new { question = "What is AI?" },
    output: new { answer = "AI is..." }
);
```

### 6. Analytics
```csharp
var usage = await client.Analytics.GetUsageAsync(
    fromDate: DateTime.Now.AddDays(-30),
    toDate: DateTime.Now,
    granularity: "day"
);
```

## Backwards Compatibility Notes

Include in documentation:
- `client.Indexes` still works but is deprecated
- Migration guide from v1 to v2
- `[Obsolete]` warnings will appear for Indexes usage
- Recommend using `client.Knowledge` for new code

## Documentation Priority

1. **High Priority** (User-facing):
   - README.md
   - Swedish Professional Presentation
   - QuickStart-Guide.md

2. **Medium Priority**:
   - SDK-Usage-Examples.md
   - Other presentation files

3. **Low Priority**:
   - Complete Overview (technical deep-dive)
   - Convert-To-PowerPoint.md

## Testing Documentation

After updates, verify:
- All code examples are syntactically correct
- API calls match v2 endpoints
- No references to deprecated v1 concepts remain
- New features are properly explained
- Swedish translations are accurate

---

**Next Steps**: Update each document according to this plan, test examples, and commit all changes.
