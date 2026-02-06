# OpperSharp TestConsole

Ett komplett testprogram för att testa alla funktioner i OpperSharp SDK v2 API med fokus på Agent-ramverket. Framförallt är test 1 - 3, samt 10 - 12 designade för att demonstrera och testa Agent-funktionaliteten i olika scenarier, inklusive en realistisk konsultmatchningsuse case.

## Förberedelser

### 1. Sätt API-nyckel

Programmet läser API-nyckeln från **User Secrets** (rekommenderat om man kör från Visual Studio) eller miljövariabel `OPPER_API_KEY`:

**User Secrets (när projektet öppnats i Visual Studio):**
1. Högerklicka på projektet → "Manage User Secrets"
2. Lägg till:
   ```json
   {
     "OPPER_API_KEY": "din-api-nyckel-här"
   }
   ```

**Miljövariabel (alternativ):**

**Windows (PowerShell):**
```powershell
$env:OPPER_API_KEY="din-api-nyckel-här"
```

eller öppna "Environment Variables" i Windows (klicka på Start eller använd Windows-tangenten, och skriv 'env', klicka sedan på 'Edit environment variables for your account') och lägg till `OPPER_API_KEY` där.

**Linux/Mac:**
```bash
export OPPER_API_KEY="din-api-nyckel-här"
```

### 2. Bygg och kör

```bash
cd src/UI/OpperSharp.UI.TestConsole
dotnet run
```

## Viktigt: Ad-hoc Function Calls

**OpperSharp använder nu ad-hoc function calls** - du behöver INTE skapa några named functions i Opper Dashboard!

Alla funktioner (name, instructions, model) skickas i request body. Detta gör SDK:n enklare att använda och mer flexibel.

## Funktioner i Menyn

### AGENT TESTS

**1) Agent: Basic Math**
- Demonstrerar Agent-ramverket med matematik-verktyg
- Agenten kan använda Add, Multiply, Percentage, Divide, Subtract
- Verktyg upptäcks automatiskt via `[Tool]` attribute
- Testar 4 matematiska problem automatiskt
- Visar execution trace med tool calls
- Visar realtidsuppdateringar med iterationsnummer och anropade verktyg

**2) Agent: Custom Tools**
- Demonstrerar anpassade verktyg skapade med `AgentTool.Create()`
- `query_database` - Simulerad databas som returnerar användarprofiler
- `get_weather` - Simulerat väder-API
- Visar hur man skapar verktyg med lambda-funktioner
- Case-insensitive tool name matching
- Realtidsvisning av vilka verktyg som anropas

**3) Agent: Multi-step**
- Demonstrerar komplex problemlösning med flera steg
- Beräknar totala lönekostnader för 3 avdelningar + arbetsgivaravgift
- Kräver flera verktygsanrop i sekvens
- Visar execution trace för att följa agentens resonemang
- Demonstrerar hur agenten bryter ner komplexa problem
- Realtidsuppdateringar visar iteration och använda verktyg

### BASIC API TESTS

**4) Simple Function Call**
- Testar grundläggande ad-hoc function call
- Du kan ställa en fråga interaktivt
- Visar token-användning och caching
- Använder claude-sonnet-4 model

**5) Streaming Response**
- Genererar en kort berättelse baserat på ditt ämne
- OBS: Streaming fungerar inte med ad-hoc calls i nuvarande implementation
- Använder istället standard CallAsync
- Visar creative writing med temperature 0.8

**6) Conversational Chat**
- Interaktiv konversation via ad-hoc function calls
- Bygger upp en konversationshistorik
- Skriv 'quit' för att avsluta
- Demonstrerar stateful multi-turn conversations

### v2 API TESTS

**7) Knowledge Base**
- Skapar en kunskapsbas med unique timestamp name
- Skapar och laddar upp en testfil (oppersharp-info.txt)
- Listar uppladdade filer med storlek
- Demonstrerar filbaserad RAG-funktion
- Visar korrekt hantering av `original_filename` från API

**8) Embeddings**
- Genererar vector embeddings för 3 texter
- Använder azure/text-embedding-3-large model
- Visar dimensioner (typiskt 3072)
- Beräknar similarity mellan första två texterna (dot product)
- Demonstrerar batch embedding generation

**9) Model Aliases**
- Listar tillgängliga modeller
- Skapar model alias med fallback-kedja (Opus → Sonnet → Haiku)
- Listar alla aliases i kontot
- Raderar test-aliases automatiskt (cleanup)
- Demonstrerar reliability via automatic fallbacks

### CONSULTANT MATCHING

**10) Match Consultant to Assignment**
- Grundläggande konsultmatchning med 3 konsulter
- Agent med två verktyg: `get_consultants` och `calculate_match_score`
- Analyserar svenskt uppdrag (.NET 8 modernisering)
- Beräknar matchningspoäng baserat på skills, experience, rate
- Ger rankad rekommendation med motivering
- **Fungerar med 4 tool calls i 3 iterations**
- Visar realtidsuppdateringar med agentens tankegång

**11) Scale Test: 25 Consultants with Rich CV Data**
- Test av agent-prestanda med realistisk datamängd
- 25 konsultprofiler med:
  - Detaljerade skills (7+ per konsult)
  - Projekt-historik (3 projekt per konsult)
  - Certifieringar
  - Språkkunskaper
  - Availability och rates
- Agent analyserar och väljer de mest lovande kandidaterna
- **Optimerad: 6 tool calls i 3 iterations** (får data → analyserar → scorar top 5)
- Demonstrerar skalbarhet för produktionsmiljö
- Realtidsvisning av iteration, verktyg och agentens resonemang

**12) Error Handling: Tools with Failures**
- Testar agent-resiliens när verktyg misslyckas
- Tre unreliable tools med olika failure rates:
  - `unreliable_data_fetch` - 30% failure rate
  - `flaky_calculation` - 25% failure rate
  - `slow_service` - 20% timeout rate
- Agenten hanterar exceptions gracefully
- Visar hur man bygger robusta agenter
- **Fungerar perfekt med 3 tool calls**
- Konsekvent beteende med Temperature = 0.0
- Realtidsuppdateringar under exekvering

### DEBUG UTILITIES

**94) Test Ad-Hoc Function Call**
- Direkt HTTP POST till /v2/call
- Demonstrerar ad-hoc mode (ingen named function)
- Raw request/response inspection

**95) Get Function Details**
- Hämtar detaljer för en specifik function
- Visar UUID, path, name, instructions, model
- Demonstrerar både UUID och path-baserat anrop

**96) Delete all 7 Functions**
- Cleanup utility (om du ändå skapade named functions)
- Raderar: math-solver, research-agent, problem-solver, general-qa, story-generator, chat-assistant, consultant-matcher

**97) Create all 7 Functions via API**
- Skapar named functions programmatiskt
- OBS: Inte nödvändigt för ad-hoc mode!
- Använd endast om du vill testa named function mode

**98) Initialize/Activate all 7 Functions**
- Anropar varje function en gång för att aktivera
- OBS: Inte nödvändigt för ad-hoc mode!

**99) List all Functions**
- Listar alla functions i ditt Opper-konto
- Visar raw JSON response
- Debug utility för att inspektera function state

## Arkitektur: Konsultmatchning

### Grundläggande (Menu 10)

```csharp
var agent = new Agent(client, new AgentOptions
{
    Name = "consultant-matcher",
    OnProgress = (update) =>
    {
        WriteLine($"\n[Iteration {update.Iteration}]");
        if (update.ToolCallCount > 0)
        {
            WriteLine($"  💡 Anropar {update.ToolCallCount} verktyg: {string.Join(", ", update.ToolNames)}");
        }
        if (!string.IsNullOrWhiteSpace(update.Message))
        {
            WriteLine($"  🤔 Agent: {update.Message.Substring(0, 150)}...");
        }
    },
    Instructions = "You are an AI consultant matching system...",
    MaxIterations = 10,
    Model = "anthropic/claude-opus-4.5"
})
.WithTool(AgentTool.Create(
    name: "get_consultants",
    description: "Retrieves consultant profiles",
    handler: (string _) => {
        return Task.FromResult(JsonSerializer.Serialize(consultants));
    },
    inputDescription: "Any query string"
))
.WithTool(AgentTool.Create(
    name: "calculate_match_score",
    description: "Calculates match score (0-100)",
    handler: (string input) => {
        var obj = JsonSerializer.Deserialize<Dictionary<string, string>>(input);
        var consultantName = obj["consultantName"];
        // Scoring logic...
        return Task.FromResult(score.ToString());
    },
    inputDescription: "JSON: {\"consultantName\": \"Name\", \"requirements\": \"...\"}"
));

var response = await agent.RunAsync("Analyze this assignment...");
```

### Scale Test (Menu 11)

- 25 konsulter med IDs (C001-C025)
- Använder `consultantId` istället för `consultantName`
- Agenten analyserar och väljer top 5-6 kandidater att scora
- Optimerad för att undvika onödiga tool calls

### Viktiga Lärdomar från Menu 11 Optimization

**Vad som fungerar:**
- XML examples i instructions: `<function_calls><invoke name="tool"><parameter>...</parameter></invoke></function_calls>`
- CRITICAL RULES sektion som varnar mot hallucination
- WORKFLOW med numbered steps
- JSON input för tools som behöver flera parametrar
- Ge agenten autonomi att välja vilka konsulter som ska scoras

**Vad som INTE fungerar:**
- Försöka tvinga agenten att kalla specific verktyg med hard-coded inputs
- Använda "Call tool_name with input X" i user query (triggar description mode)
- För aggressiva "EXECUTE NOW" instruktioner
- Försöka efterlikna Menu 12's pattern när uppgiften är fundamentalt annorlunda

## Realtidsvisning av Agentens Tankegång

Alla agenttester (Menu 1-3, 10-12) visar nu realtidsuppdateringar under exekvering:

**OnProgress Callback:**
```csharp
OnProgress = (update) =>
{
    WriteLine($"[Iteration {update.Iteration}]");
    if (update.ToolCallCount > 0)
    {
        WriteLine($"💡 Anropar {update.ToolCallCount} verktyg: {string.Join(", ", update.ToolNames)}");
    }
    if (!string.IsNullOrWhiteSpace(update.Message))
    {
        WriteLine($"🤔 Agent: {update.Message.Substring(0, 150)}...");
    }
}
```

**Fördelar:**
- Användare ser vad AI:n tänker i realtid
- Visar vilka verktyg som anropas
- Ger interaktiv feedback under långvariga operationer
- Hjälper till att förstå agentens problemlösningsprocess

## Exempel: Production Integration

### 1. Integrera med databas

```csharp
.WithTool(AgentTool.Create(
    name: "get_consultants",
    description: "Query CINode API for available consultants",
    handler: async (string query) =>
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", cinodeApiKey);

        var response = await httpClient.GetAsync(
            "https://api.cinode.com/v1/consultants?available=true"
        );
        var consultants = await response.Content.ReadAsStringAsync();
        return consultants;
    },
    inputDescription: "Search query for consultants (e.g., 'available', 'C# developers')"
))
```

### 2. Förbättrad matchningslogik

```csharp
handler: async (string input) =>
{
    var obj = JsonSerializer.Deserialize<Dictionary<string, string>>(input);
    var consultantId = obj["consultantId"];
    var requirements = obj["requirements"];

    // Använd Embeddings för semantisk matchning
    var reqEmbedding = await client.Embeddings.CreateAsync(requirements);
    var consultantProfile = await GetConsultantProfile(consultantId);
    var conEmbedding = await client.Embeddings.CreateAsync(consultantProfile);

    // Cosine similarity
    var similarity = CalculateCosineSimilarity(reqEmbedding, conEmbedding);

    // Kombinera med regelbaserad scoring
    var score = (similarity * 50) + RuleBasedScore(consultant, requirements);

    return score.ToString();
}
```

### 3. Lägg till fler verktyg

```csharp
.WithTool(AgentTool.Create(
    name: "check_availability",
    description: "Check consultant's calendar availability",
    handler: async (string consultantId) => { /* ... */ }
))
.WithTool(AgentTool.Create(
    name: "get_previous_assignments",
    description: "Get consultant's assignment history",
    handler: async (string consultantId) => { /* ... */ }
))
.WithTool(AgentTool.Create(
    name: "calculate_travel_distance",
    description: "Calculate distance between consultant and assignment location",
    handler: async (string input) => { /* ... */ }
))
```

### 4. Använd Knowledge Base för RAG

```csharp
// Ladda upp alla uppdragsbeskrivningar
var kb = await client.Knowledge.CreateAsync("assignments-kb");
foreach (var assignment in assignments)
{
    await client.Knowledge.UploadFileAsync(kb.Id, assignment.Title, content);
}

// Använd i agent
.WithTool(AgentTool.Create(
    name: "search_similar_assignments",
    description: "Find similar past assignments using RAG",
    handler: async (string query) =>
    {
        // RAG query mot knowledge base
        var results = await client.Knowledge.QueryAsync(kb.Id, query);
        return JsonSerializer.Serialize(results);
    }
))
```

## Performance Tips

### Menu 11 Optimization Insights

- **6 tool calls är optimalt** för 25 konsulter (1 get + 5 score)
- Agenten gör naturlig pre-filtering baserat på requirements
- Tvinga inte specifika tool call patterns - ge agenten autonomi
- XML examples i instructions är kritiska för att undvika hallucination
- JSON input format fungerar bättre än simple strings för complex data

### Menu 12 Konsistens

- **Temperature = 0.0** säkerställer deterministiskt beteende
- Utan Temperature = 0.0 hade Menu 12 endast 25% framgångsfrekvens (0 tool calls 3 av 4 gånger)
- XML examples i instructions förhindrar hallucination
- CRITICAL RULES och WORKFLOW sektioner ger tydlig vägledning

### General Best Practices

1. **MaxIterations**: Sätt till 10-15 för komplexa uppgifter
2. **Model**: Använd claude-opus-4.5 för agents (bättre reasoning)
3. **Instructions**: Inkludera CRITICAL RULES och XML examples
4. **Error Handling**: Se Menu 12 för resilient tool design
5. **Tool Descriptions**: Var specifik om input/output format
6. **Temperature**: Använd 0.0 för konsekvent, deterministiskt beteende
7. **OnProgress**: Lägg till callback för realtidsfeedback till användare

## Felsökning

### "Could not initialize OpperClient"
- Kontrollera User Secrets eller OPPER_API_KEY miljövariabel
- Visual Studio: Högerklicka projekt → Manage User Secrets
- Verify: `dotnet user-secrets list`

### Agent hallucination (0 tool calls)
- Kontrollera att instructions innehåller XML examples
- Se till att tool inputDescription matchar handler signature
- Menu 10 och 12 är working examples att kopiera från
- För Menu 12: Säkerställ att Temperature = 0.0 är satt
- Undvik att använda "Call tool_name with input X" språk i user queries

### "Tool 'xxx' not found"
- Case-insensitive matching är aktiverat
- Kontrollera stavning i WithTool() och tool name i handler
- Use AgentTool.Create() för simple tools

### Performance issues
- För många consultants? Implementera server-side filtering
- Too many iterations? Justera MaxIterations eller instruktioner
- Se Menu 11 för exempel på effektiv large-scale matching

### Inkonsekvent beteende
- Sätt Temperature = 0.0 för deterministiskt beteende
- LLM:er är stokastiska - utan Temperature = 0.0 kan samma input ge olika outputs
- Menu 12 demonstrerade detta: 0 tool calls 75% av tiden tills Temperature sattes till 0.0

## Nästa Steg

1. ✅ **Testa alla funktioner** - Gå igenom meny 1-12 systematiskt
2. ✅ **Förstå Agent patterns** - Menu 10 (basic), 11 (scale), 12 (errors)
3. 🔄 **Anpassa Menu 11** - Byt ut hårdkodade konsulter mot CINode API
4. 🔄 **Förbättra scoring** - Lägg till embeddings för semantisk matchning
5. 🔄 **Lägg till verktyg** - Availability, history, distance, etc.
6. 🔄 **Bygg GUI** - Skapa användargränssnitt för konsultmatchning

## Resurser

- **OpperSharp Documentation**: `../../doc/`
- **Agent Framework**: Se Menu 1-3, 10-12 för examples
- **Opper API Docs**: https://docs.opper.ai
- **Claude Models**: https://docs.anthropic.com/claude/docs

---

**English version available**: [README.md](README.md)
