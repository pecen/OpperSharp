# Knowledge Base (RAG) och Model Aliases - Förklaring

## Knowledge Base (Retrieval-Augmented Generation)

### Vad är Knowledge Base i Opper?

**Kort svar:** En Knowledge Base är en filbaserad databas där du kan ladda upp dokument som AI:n kan söka igenom och använda för att svara på frågor.

**Längre förklaring:**
Knowledge Base i Opper är ett RAG-system (Retrieval-Augmented Generation) som gör det möjligt för AI:n att svara på frågor baserat på dina egna dokument, istället för bara sin träningsdata.

### Hur fungerar det?

```
1. Du laddar upp dokument (PDF, TXT, etc.)
   ↓
2. Opper skapar embeddings för dokumenten
   ↓
3. När användare ställer en fråga:
   - Frågan konverteras till en embedding
   - Systemet hittar relevanta delar i dokumenten
   - AI:n får kontexten och genererar svar
```

### Praktiskt Exempel

**Scenario:** Du har 100 interna policydokument

**Utan Knowledge Base:**
```
Användare: "Vad är vår policy för distansarbete?"
AI: "Jag har ingen information om ert företags specifika policy..."
```

**Med Knowledge Base:**
```
Användare: "Vad är vår policy för distansarbete?"
AI: "Enligt er HR-policy (uppdaterad 2024-01-15) tillåter företaget
     upp till 3 dagars distansarbete per vecka. Medarbetare måste..."
```

### Användningsområden

#### 1. **Intern Kunskapsbas**
- HR-policyer och riktlinjer
- Teknisk dokumentation
- Produktmanualer
- FAQ-dokument

#### 2. **Kundtjänst**
- Automatisk support baserad på produktdokumentation
- Företagsspecifika svar
- Uppdaterad information (inte begränsad till AI:ns träningsdata)

#### 3. **Konsultmatchning**
- Ladda upp alla konsult-CV:n
- Sök efter bästa matchning mot uppdragsbeskrivning
- AI kan referera till specifika erfarenheter i CV:na

#### 4. **Analys och Research**
- Ladda upp forskningsartiklar
- Ställ frågor om innehållet
- Få sammanfattningar och insikter

### I OpperSharp TestConsole (Menyval 7)

Testet visar hela flödet:

```csharp
// 1. Skapa Knowledge Base
var kb = await client.Knowledge.CreateAsync(
    name: "test-kb",
    embeddingModel: "azure/text-embedding-3-large"
);

// 2. Ladda upp fil
using var fileStream = File.OpenRead("dokument.txt");
await client.Knowledge.UploadFileAsync(
    knowledgeBaseId: kb.Id,
    filename: "dokument.txt",
    fileStream: fileStream,
    contentType: "text/plain"
);

// 3. Lista filer i KB
var files = await client.Knowledge.ListFilesAsync(kb.Id);

// 4. Använd med Functions (RAG)
// Skapa en function som har access till denna KB
```

### Tekniska Detaljer

**Filtyper som stöds:**
- Text (.txt, .md)
- PDF (.pdf)
- Office-dokument (.docx, .xlsx, osv.)
- Kod-filer (.cs, .js, .py, osv.)

**Embedding-modeller:**
- `azure/text-embedding-3-small` - Snabbare, billigare
- `azure/text-embedding-3-large` - Bättre kvalitet (rekommenderas)

**Storlek:**
- Enskild fil: Upp till 10 MB
- Total KB-storlek: Beror på Opper-plan

### Best Practices

✅ **Strukturera dokument tydligt**
- Använd headings och sektioner
- Tydliga avsnitt gör det lättare för AI:n att hitta rätt information

✅ **Uppdatera regelbundet**
- Ta bort gamla versioner av dokument
- Håll informationen aktuell

✅ **Använd beskrivande filnamn**
- "HR-Policy-2024.pdf" istället för "dokument1.pdf"

✅ **Kombinera med metadata**
- Använd metadata för att kategorisera dokument
- Hjälper vid filtrering och sökning

⚠️ **Undvik duplicerad information**
- Samma information i flera dokument kan ge förvirrande svar

### Konsultmatchning med Knowledge Base

**Steg-för-steg:**

```csharp
// 1. Skapa KB för konsulter
var consultantsKB = await client.Knowledge.CreateAsync(
    name: "consultants-database",
    embeddingModel: "azure/text-embedding-3-large"
);

// 2. Ladda upp alla konsult-CV:n
foreach (var cvFile in Directory.GetFiles("cvs/"))
{
    using var stream = File.OpenRead(cvFile);
    await client.Knowledge.UploadFileAsync(
        knowledgeBaseId: consultantsKB.Id,
        filename: Path.GetFileName(cvFile),
        fileStream: stream,
        contentType: "application/pdf"
    );
}

// 3. Skapa en Function som använder denna KB
// I Opper Dashboard:
// - Skapa function "find-consultant"
// - Koppla till consultants-database KB
// - Prompt: "Du är en rekryteringsassistent..."

// 4. Använd funktionen
var response = await client.Functions.CallAsync(
    path: "find-consultant",
    input: new Dictionary<string, object>
    {
        ["assignment"] = "Vi söker en senior C# utvecklare med Azure-erfarenhet..."
    }
);
```

---

## Model Aliases (Automatisk Fallback)

### Vad är Model Aliases?

**Kort svar:** Model Aliases är ett sätt att skapa pålitliga AI-anrop med automatisk fallback om en modell är otillgänglig.

**Längre förklaring:**
Ett Model Alias är ett alias-namn som pekar på en lista av AI-modeller. Om den första modellen misslyckas eller är överbelastad, prövar Opper automatiskt nästa modell i listan.

### Varför behövs detta?

**Problem utan aliases:**
```
Din app: Anropar "gpt-4"
OpenAI: "503 Service Unavailable"
Din app: ❌ KRASCH - användaren ser ett felmeddelande
```

**Lösning med aliases:**
```
Din app: Anropar "my-reliable-gpt4" (alias)
OpenAI gpt-4: "503 Service Unavailable"
  ↓ Automatisk fallback
OpenAI gpt-4-turbo: "200 OK" ✅
  ↓
Din app: Fungerar utan avbrott!
```

### Praktiskt Exempel

```csharp
// Skapa ett alias med fallback-kedja
var alias = await client.Models.CreateAliasAsync(
    name: "production-gpt4",
    fallbackModels: new List<string>
    {
        "openai/gpt-4o",           // Pröva först
        "openai/gpt-4-turbo",      // Om #1 misslyckas
        "anthropic/claude-3-opus", // Om #2 misslyckas
        "openai/gpt-3.5-turbo"     // Sista utväg
    },
    description: "Production GPT-4 with multi-provider fallback"
);

// Använd aliaset i dina anrop
var response = await client.Functions.CallAsync(
    path: "general-qa",
    input: new Dictionary<string, object> { ["question"] = "..." },
    options: new OpperCallOptions
    {
        Model = "production-gpt4"  // Använd alias istället för specifik modell
    }
);
```

### Fördelar med Model Aliases

#### 1. **Hög Tillgänglighet (Uptime)**
- Om OpenAI har problem → byt till Anthropic
- Om GPT-4 är överbelastad → byt till GPT-3.5
- 99.9% uptime även om enskilda providers har problem

#### 2. **Kostnadsoptimering**
```csharp
// Fallback från dyr till billigare modell
fallbackModels: new[]
{
    "openai/gpt-4o",        // $10/1M tokens
    "openai/gpt-3.5-turbo"  // $1/1M tokens (10x billigare)
}
```

#### 3. **A/B-testning**
```csharp
// Testa olika modeller utan att ändra kod
// Växla mellan GPT-4 och Claude genom att ändra alias
```

#### 4. **Enkel Migration**
```csharp
// Byt från GPT-4 till Claude 3.5 för hela företaget
// Ändra bara alias-konfigurationen, inte koden
```

### Modellformat i Opper

Modeller i Opper använder formatet `provider/model-name`:

**OpenAI:**
- `openai/gpt-4o`
- `openai/gpt-4-turbo`
- `openai/gpt-3.5-turbo`
- `openai/gpt-4o-mini`

**Anthropic:**
- `anthropic/claude-3-opus`
- `anthropic/claude-3-sonnet`
- `anthropic/claude-3-haiku`
- `anthropic/claude-3.5-sonnet`

**Google:**
- `google/gemini-pro`
- `google/gemini-1.5-pro`

**Groq (snabba modeller):**
- `groq/llama-3-70b`
- `groq/mixtral-8x7b`

**Azure:**
- `azure/gpt-4o`
- `azure/gpt-35-turbo`

### Användningsområden

#### 1. **Production Applications**
```csharp
// Kritiska produktionssystem
fallbackModels: new[]
{
    "openai/gpt-4o",
    "anthropic/claude-3-opus",
    "openai/gpt-3.5-turbo"
}
```

#### 2. **Kostnadsmedveten Utveckling**
```csharp
// Dev: Billiga modeller
// Prod: Bästa modeller med fallback
fallbackModels: new[]
{
    "openai/gpt-4o",
    "openai/gpt-3.5-turbo",
    "groq/llama-3-70b"  // Gratis/billigt för överskott
}
```

#### 3. **Region-specifika Fallbacks**
```csharp
// EU → Azure (GDPR-compliant)
// US → OpenAI direkt
fallbackModels: new[]
{
    "azure/gpt-4o",     // EU-hosted
    "openai/gpt-4o"     // US fallback
}
```

#### 4. **Hastighetoptimering**
```csharp
// Snabba modeller först
fallbackModels: new[]
{
    "groq/llama-3-70b",      // Extremt snabb
    "openai/gpt-4o-mini",    // Snabb och billig
    "openai/gpt-4o"          // Bästa kvalitet men långsammare
}
```

### I OpperSharp TestConsole (Menyval 9)

Testet demonstrerar:

```csharp
// 1. Skapa alias
var alias = await client.Models.CreateAliasAsync(
    name: "test-reliable-gpt4",
    fallbackModels: new List<string>
    {
        "openai/gpt-4o",
        "openai/gpt-4-turbo",
        "openai/gpt-3.5-turbo"
    },
    description: "Test alias with GPT-4 fallback chain"
);

// 2. Lista alla aliases
var aliases = await client.Models.ListAliasesAsync();

// 3. Radera test-alias
await client.Models.DeleteAliasAsync(aliasName);
```

### Best Practices

✅ **Sortera efter prioritet**
- Bästa modell först
- Billigare/snabbare som fallback

✅ **Testa fallback-kedjan**
- Verifiera att alla modeller i kedjan faktiskt fungerar
- Testa att fallback händer korrekt

✅ **Använd beskrivande namn**
- `production-gpt4` istället för `alias1`
- Tydligt vad aliaset används till

✅ **Dokumentera fallback-strategin**
- Varför dessa modeller?
- I vilken ordning?
- Vad är trade-offs:en?

⚠️ **Överväg kvalitetsskillnader**
- GPT-4 → GPT-3.5 kan ge sämre svar
- Testa om fallback påverkar användarupplevelsen

### Avancerat: Provider-diversity

**Bästa praxis för mission-critical system:**

```csharp
fallbackModels: new[]
{
    "openai/gpt-4o",              // Provider 1
    "anthropic/claude-3-opus",    // Provider 2 (annan leverantör!)
    "google/gemini-1.5-pro",      // Provider 3 (annan leverantör!)
    "groq/llama-3-70b"            // Provider 4 (gratis fallback)
}
```

**Varför?**
- Om OpenAI har problem påverkar det inte Anthropic
- Olika providers har olika styrkor
- Diversifiering = mindre risk

### Sammanfattning för Presentation

**Knowledge Base (RAG):**
- 📚 Ladda upp egna dokument
- 🔍 AI söker och hittar relevant information
- 💡 Svar baserade på företagets data, inte bara träningsdata
- ✅ Perfekt för intern dokumentation, kundtjänst, konsultmatchning

**Model Aliases:**
- 🔄 Automatisk fallback mellan AI-modeller
- 🚀 Hög tillgänglighet (99.9% uptime)
- 💰 Kostnadsoptimering (fallback till billigare modeller)
- 🛡️ Risk-diversifiering (flera providers)

### Kod-exempel för Presentation

**Knowledge Base:**
```csharp
// Skapa, ladda upp, använd
var kb = await client.Knowledge.CreateAsync("min-kb", "azure/text-embedding-3-large");
await client.Knowledge.UploadFileAsync(kb.Id, "dok.pdf", fileStream);
// Använd med Functions för RAG
```

**Model Aliases:**
```csharp
// Skapa pålitlig fallback-kedja
var alias = await client.Models.CreateAliasAsync(
    "production-ai",
    new[] { "openai/gpt-4o", "anthropic/claude-3-opus", "openai/gpt-3.5-turbo" }
);
// Använd i alla anrop för automatisk fallback
```

### Vidare Läsning

**Opper Dokumentation:**
- Knowledge Base API: https://docs.opper.ai/api-reference/knowledge/overview
- Model Aliases: https://docs.opper.ai/api-reference/models/overview
- RAG Best Practices: https://docs.opper.ai/patterns/rag

**Externa Resurser:**
- RAG Architecture: https://www.anthropic.com/research/retrieval-augmented-generation
- Model Comparison: https://artificialanalysis.ai/
