# OpperSharp TestConsole

Ett komplett testprogram för att testa alla funktioner i OpperSharp SDK v2 API.

## Förberedelser

### 1. Sätt API-nyckel

Programmet läser API-nyckeln från miljövariabeln `OPPER_API_KEY`:

**Windows (PowerShell):**
```powershell
$env:OPPER_API_KEY="din-api-nyckel-här"
```

**Windows (CMD):**
```cmd
set OPPER_API_KEY=din-api-nyckel-här
```

**Linux/Mac:**
```bash
export OPPER_API_KEY="din-api-nyckel-här"
```

### 2. Skapa nödvändiga Functions i Opper

För att vissa tester ska fungera behöver du skapa följande functions i Opper-plattformen:

**Required Functions (för Agent-tester):**
- `math-solver` - En funktion för matematiska problem
- `research-agent` - En funktion för research/queries
- `problem-solver` - En funktion för komplex problemlösning
- `consultant-matcher` - En funktion för konsultmatchning

**Optional Functions (för övriga tester):**
- `general-qa` - En Q&A funktion (används i menyval 4)
- `story-generator` - En berättelse-generator (används i menyval 5)

**Tips för Function-skapande i Opper:**
1. Gå till Opper Dashboard
2. Skapa en ny Function
3. Sätt `path` enligt ovan (t.ex. "math-solver")
4. Lägg till en enkel prompt, t.ex: "You are a helpful assistant that solves math problems."
5. Välj modell (rekommenderat: gpt-4)

### 3. Bygg och kör

```bash
cd src/UI/OpperSharp.UI.TestConsole
dotnet run
```

## Funktioner i Menyn

### AGENT TESTS (Prioritet)

**1) Agent: Basic Math**
- Demonstrerar Agent-ramverket med matematik-verktyg
- Agenten kan använda Add, Multiply, Percentage, Divide, Subtract
- Testar flera matematiska problem automatiskt
- Visar hur verktyg anropas och används

**2) Agent: Custom Tools**
- Demonstrerar anpassade verktyg
- Simulerad databas-query (get_consultants)
- Simulerat väder-API (get_weather)
- Visar hur man skapar egna verktyg med `AgentTool.Create()`

**3) Agent: Multi-step**
- Demonstrerar komplex problemlösning med flera steg
- Löser ett problem som kräver flera verktygsanrop
- Visar execution trace för att följa agentens resonemang

### BASIC API TESTS

**4) Simple Function Call**
- Testar grundläggande function-anrop
- Du kan ställa en fråga interaktivt
- Visar token-användning och caching

**5) Streaming Response**
- Demonstrerar streaming av svar
- Genererar en kort berättelse baserat på ditt ämne
- Visar text allt eftersom den genereras

**6) Chat API**
- Interaktiv konversation
- Bygger upp en konversationshistorik
- Skriv 'quit' för att avsluta

### v2 API TESTS

**7) Knowledge Base**
- Skapar en kunskapsbas
- Laddar upp en testfil
- Listar uppladdade filer
- Demonstrerar den filbaserade RAG-funktionen i v2

**8) Embeddings**
- Genererar vector embeddings för 3 texter
- Visar dimensioner och värden
- Beräknar likhet mellan texter

**9) Model Aliases**
- Skapar ett model alias med fallback-kedja
- Listar alla aliases
- Raderar test-alias

### CONSULTANT MATCHING

**C) Match Consultant to Assignment**
- **Detta är ditt slutmål!**
- Demonstrerar en komplett konsultmatchningslösning
- Agenten analyserar ett uppdrag
- Agenten använder verktyg för att hämta konsultprofiler
- Agenten beräknar matchningspoäng
- Ger en rankad rekommendation med motivering

## Exempel: Konsultmatchning

Konsultmatchningen fungerar så här:

1. **Uppdragsbeskrivning** definieras (hårdkodad i exemplet)
2. **Konsultprofiler** finns tillgängliga (3 st i exemplet)
3. **Agent skapas** med två verktyg:
   - `get_consultants` - Hämtar alla konsultprofiler
   - `calculate_match_score` - Beräknar hur väl en konsult passar
4. **Agenten analyserar** uppdraget och kör verktyg
5. **Resultat** presenteras med motivering

### Utöka för produktion

För att använda detta i produktion:

1. **Byt ut hårdkodade konsulter** mot databas-query:
   ```csharp
   .WithTool(AgentTool.Create(
       name: "get_consultants",
       description: "Query database for available consultants",
       handler: async () =>
       {
           using var dbContext = new YourDbContext();
           var consultants = await dbContext.Consultants
               .Where(c => c.Available)
               .ToListAsync();
           return JsonSerializer.Serialize(consultants);
       }
   ))
   ```

2. **Förbättra matchningslogik** med vektorbaserad sökning:
   ```csharp
   // Använd Embeddings API för semantisk matchning
   var assignmentEmbedding = await _client.Embeddings.CreateAsync(assignment);
   var consultantEmbedding = await _client.Embeddings.CreateAsync(consultantProfile);
   // Beräkna cosine similarity
   ```

3. **Lägg till fler verktyg:**
   - `check_availability` - Kolla konsultkalender
   - `get_previous_assignments` - Hämta tidigare uppdrag
   - `calculate_travel_distance` - Beräkna reseavstånd
   - `estimate_project_cost` - Beräkna totalkostnad

4. **Använd Knowledge Base** för uppdragsbeskrivningar:
   ```csharp
   // Ladda upp alla uppdragsbeskrivningar till en knowledge base
   // Använd RAG för att hitta liknande tidigare uppdrag
   ```

## Felsökning

### "Could not initialize OpperClient"
- Kontrollera att `OPPER_API_KEY` är satt som miljövariabel
- Kör `echo $env:OPPER_API_KEY` (PowerShell) för att verifiera

### "Function 'xxx' not found"
- Logga in på Opper Dashboard
- Skapa function med rätt path
- Kontrollera att path:en matchar exakt (case-sensitive)

### Agent-test fungerar inte
- Kontrollera att du har skapat motsvarande function i Opper
- Funktionen måste ha samma `path` som anges i koden
- Funktionen måste ha en grundläggande prompt

### Network errors
- Kontrollera internetanslutning
- Kontrollera att Opper API är tillgängligt
- Vissa företagsnätverk kan blockera AI-tjänster

## Nästa steg

1. **Testa alla funktioner** - Gå igenom menyn systematiskt
2. **Modifiera konsultmatchning** - Anpassa till dina behov
3. **Lägg till verktyg** - Skapa egna verktyg för dina use cases
4. **Integrera med databas** - Koppla till din konsultdatabas
5. **Bygg GUI** - Skapa ett användargränssnitt ovanpå detta

## Resurser

- **OpperSharp Documentation**: `../../doc/`
- **QuickStart Guide**: `../../doc/QuickStart-Guide.md`
- **SDK Examples**: `../../doc/SDK-Usage-Examples.md`
- **Opper API Docs**: https://docs.opper.ai
