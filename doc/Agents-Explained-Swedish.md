# Agents - Förklaring på Enkelt Sätt

## Vad är en Agent?

**Kort svar:** En Agent är en AI som kan använda verktyg (tools) för att lösa problem steg-för-steg.

**Längre förklaring:**
En vanlig AI-chatbot kan bara prata. En Agent kan både prata OCH utföra handlingar genom att använda verktyg du ger den.

## Skillnaden mellan vanlig AI och Agent

### Vanlig AI (Function Call):
```
Du: "Vad är 15% av 2500?"
AI: "15% av 2500 är 375"
```
AI:n *vet* svaret från sin träningsdata.

### Agent med verktyg:
```
Du: "Vad är 15% av 2500?"
Agent tänker: "Jag behöver beräkna detta..."
Agent använder: percentage(2500, 15)  ← Verktyget du gav den!
Verktyget svarar: 375
Agent: "Svaret är 375"
```
Agenten *använder verktyg* för att lösa problemet.

## Varför är detta användbart?

### 1. **Aktuell information**
```
Vanlig AI: "Jag vet inte aktuellt väder" (träningsdata från 2023)
Agent: Anropar get_weather("Stockholm") → "Just nu 5°C och molnigt"
```

### 2. **Komplex problemlösning**
```
Uppgift: "Hitta bästa konsulten för C#-projektet"

Agent:
1. Anropar get_consultants() → Hämtar alla konsulter
2. Anropar calculate_match("Anna", requirements) → Score: 0.85
3. Anropar calculate_match("Erik", requirements) → Score: 0.72
4. Anropar calculate_match("Sara", requirements) → Score: 0.91
5. Svarar: "Sara är bäst matchad (91%)"
```

### 3. **Interaktion med externa system**
- Databaser
- API:er
- Filsystem
- Beräkningar
- Företagsspecifika verktyg

## Hur fungerar det?

### 1. Du skapar en Agent
```csharp
var agent = new Agent(client, new AgentOptions
{
    FunctionPath = "math-solver",  // Function i Opper
    MaxIterations = 10,             // Max antal steg
    Model = "gpt-4"
});
```

### 2. Du ger den verktyg
```csharp
// Alternativ 1: Med [Tool] attribut
public class MathTools
{
    [Tool("Adds two numbers")]
    public double Add(double a, double b) => a + b;

    [Tool("Multiplies two numbers")]
    public double Multiply(double a, double b) => a * b;
}

agent.WithTools(new MathTools());
```

```csharp
// Alternativ 2: Med AgentTool.Create
agent.WithTool(AgentTool.Create(
    name: "get_weather",
    description: "Gets current weather for a city",
    handler: (string city) =>
    {
        // Din logik här - kolla väder-API
        return Task.FromResult($"Weather in {city}: Sunny, 20°C");
    }
));
```

### 3. Du kör Agenten
```csharp
var response = await agent.RunAsync(new Dictionary<string, object>
{
    ["problem"] = "What is 25 * 4 + 10?"
});

Console.WriteLine(response.Output);
// Agent använde: Multiply(25, 4) → 100
// Agent använde: Add(100, 10) → 110
// Svar: "110"
```

## Praktiskt Exempel: Konsultmatchning

### Scenario
Du vill hitta bästa konsulten för ett uppdrag.

### Steg 1: Definiera verktyg
```csharp
// Verktyg 1: Hämta konsulter från databas
agent.WithTool(AgentTool.Create(
    name: "get_consultants",
    description: "Retrieves all available consultants",
    handler: (string _) =>
    {
        // Hämta från databas
        var consultants = database.GetAllConsultants();
        return Task.FromResult(JsonSerializer.Serialize(consultants));
    }
));

// Verktyg 2: Beräkna matchning
agent.WithTool(AgentTool.Create(
    name: "calculate_match_score",
    description: "Calculate how well consultant matches requirements",
    handler: (string input) =>
    {
        var data = JsonSerializer.Deserialize<MatchRequest>(input);
        var score = CalculateScore(data.Consultant, data.Requirements);
        return Task.FromResult(score.ToString());
    }
));
```

### Steg 2: Kör agenten
```csharp
var query = new Dictionary<string, object>
{
    ["assignment"] = @"Vi söker en senior C# utvecklare med
                       Azure-erfarenhet för finansiellt system."
};

var response = await agent.RunAsync(query);
```

### Steg 3: Agenten arbetar automatiskt
```
Agent iteration 1:
  Tänker: "Jag behöver först hämta alla konsulter"
  Anropar: get_consultants()
  Får: Lista med 50 konsulter

Agent iteration 2:
  Tänker: "Jag behöver matcha Anna mot kraven"
  Anropar: calculate_match_score("Anna", requirements)
  Får: 0.85

Agent iteration 3:
  Anropar: calculate_match_score("Erik", requirements)
  Får: 0.72

Agent iteration 4:
  Anropar: calculate_match_score("Sara", requirements)
  Får: 0.91

Agent iteration 5:
  Tänker: "Sara har högst score"
  Svarar: "Sara Svensson är bäst matchad (91%) eftersom hon har
           8 års C#-erfarenhet, gedigen Azure-kunskap och har
           jobbat med finansiella system tidigare."
```

## Verktygstyper

### 1. **Informationshämtning**
```csharp
[Tool("Gets consultant profile")]
public string GetConsultant(string name) => database.Find(name);

[Tool("Gets current weather")]
public string GetWeather(string city) => weatherAPI.Get(city);

[Tool("Searches documentation")]
public string SearchDocs(string query) => docSearch.Search(query);
```

### 2. **Beräkningar**
```csharp
[Tool("Calculates match score")]
public double CalculateMatch(string cv, string requirements)
    => similarity.Compare(cv, requirements);

[Tool("Estimates project cost")]
public decimal EstimateCost(int hours, decimal hourlyRate)
    => hours * hourlyRate;
```

### 3. **Åtgärder**
```csharp
[Tool("Sends email notification")]
public string SendEmail(string to, string subject, string body)
    => emailService.Send(to, subject, body);

[Tool("Creates calendar event")]
public string BookMeeting(string consultant, DateTime when)
    => calendar.CreateEvent(consultant, when);
```

### 4. **Externt API**
```csharp
[Tool("Checks availability in calendar")]
public string CheckAvailability(string consultant, DateTime date)
{
    var events = calendarAPI.GetEvents(consultant, date);
    return events.Any() ? "Busy" : "Available";
}
```

## Agent vs. Vanlig Function Call

### Vanlig Function Call (Enkelt)
```
Input → AI → Output
```
Ett steg, ett svar.

**Exempel:**
```csharp
var response = await client.Functions.CallAsync(
    "general-qa",
    new { question = "Vad är Opper?" }
);
// Svar: "Opper är en AI-plattform..."
```

### Agent (Multi-step)
```
Input → AI → Tool1 → AI → Tool2 → AI → Output
```
Flera steg, agenten beslutar själv vilka verktyg den behöver.

**Exempel:**
```csharp
var response = await agent.RunAsync(
    new { task = "Find best consultant for C# project" }
);
// Agent använder flera verktyg automatiskt
```

## Best Practices

### 1. **Ge tydliga verktygsnamn och beskrivningar**
```csharp
// ✅ BRA
[Tool("Gets current weather forecast for specified city")]
public string GetWeather(string city)

// ❌ DÅLIGT
[Tool("Gets stuff")]
public string Get(string x)
```

### 2. **Begränsa MaxIterations**
```csharp
// ✅ BRA - Förhindrar oändliga loopar
new AgentOptions { MaxIterations = 10 }

// ❌ DÅLIGT - Kan fastna
new AgentOptions { MaxIterations = 1000 }
```

### 3. **Hantera fel i verktyg**
```csharp
[Tool("Gets consultant profile")]
public string GetConsultant(string name)
{
    try
    {
        return database.Find(name) ?? "Consultant not found";
    }
    catch (Exception ex)
    {
        return $"Error: {ex.Message}";
    }
}
```

### 4. **Använd EnableTracing för debugging**
```csharp
new AgentOptions
{
    EnableTracing = true  // Skapar spans i Opper för varje steg
}
```

## Verkliga Use Cases

### 1. **Kundtjänst-agent**
```
Verktyg:
- search_knowledge_base(query)
- get_customer_info(customerId)
- create_support_ticket(issue)
- check_order_status(orderId)

Agent kan:
- Svara på frågor från kunskapsbasen
- Hämta kundinformation
- Skapa ärenden
- Kolla orderstatus
```

### 2. **Konsultmatchning**
```
Verktyg:
- get_all_consultants()
- calculate_match_score(consultant, requirements)
- check_availability(consultant, dates)
- get_hourly_rate(consultant)

Agent kan:
- Hitta alla konsulter
- Matcha mot krav
- Kolla tillgänglighet
- Beräkna totalkostnad
```

### 3. **Dataanalys-agent**
```
Verktyg:
- query_database(sql)
- calculate_statistics(data)
- create_chart(data, type)
- export_to_excel(data)

Agent kan:
- Köra SQL-frågor
- Beräkna statistik
- Skapa grafer
- Exportera resultat
```

### 4. **DevOps-agent**
```
Verktyg:
- check_server_status(server)
- restart_service(service)
- check_logs(service, timerange)
- send_alert(message)

Agent kan:
- Övervaka servrar
- Starta om tjänster
- Analysera loggar
- Skicka larm
```

## Begränsningar

⚠️ **Agenten är inte perfekt:**
1. Kan göra fel i sina beslut
2. Kan fastna i loopar (använd MaxIterations)
3. Kan missförstå verktyg (ge tydliga beskrivningar)
4. Kostar mer än vanliga function calls (fler API-anrop)

⚠️ **När du INTE bör använda Agent:**
- Enkla frågor som inte behöver verktyg
- När du vet exakt vilket verktyg som ska användas
- När latency är kritiskt (agents är långsammare)

✅ **När du BÖR använda Agent:**
- Komplexa multi-step uppgifter
- När agenten behöver välja mellan flera verktyg
- När du vill att AI:n ska "tänka" och besluta
- När du inte vet exakt sekvensen av steg

## Kod-exempel: Komplett Agent

```csharp
using OpperSharp.Core;
using OpperSharp.Agents;

// 1. Skapa client
var client = OpperClient.FromEnvironment();

// 2. Skapa agent med options
var agent = new Agent(client, new AgentOptions
{
    FunctionPath = "consultant-matcher",
    MaxIterations = 10,
    Model = "anthropic/claude-opus-4.5",
    EnableTracing = true
});

// 3. Lägg till verktyg
agent.WithTool(AgentTool.Create(
    name: "get_consultants",
    description: "Retrieves all available consultants from database",
    handler: async (string _) =>
    {
        var consultants = await database.GetAllAsync();
        return JsonSerializer.Serialize(consultants);
    }
));

agent.WithTool(AgentTool.Create(
    name: "calculate_match",
    description: "Calculate match score between consultant and requirements",
    handler: async (string input) =>
    {
        var request = JsonSerializer.Deserialize<MatchRequest>(input);
        var score = await matchingService.CalculateAsync(
            request.ConsultantId,
            request.Requirements
        );
        return score.ToString();
    }
));

// 4. Kör agent
var response = await agent.RunAsync(new Dictionary<string, object>
{
    ["assignment"] = "Senior C# utvecklare med Azure för finansprojekt",
    ["start_date"] = "2024-03-01",
    ["duration"] = "6 månader"
});

// 5. Visa resultat
Console.WriteLine($"Recommendation: {response.Output}");
Console.WriteLine($"Steps taken: {response.Iterations}");
Console.WriteLine($"Tools used: {response.ToolCalls.Count}");
```

## Sammanfattning för Presentation

### ⚡ Snabba Fakta
- **Vad:** AI som kan använda verktyg för att lösa problem
- **Varför:** Utför handlingar, inte bara prata
- **Hur:** Definiera verktyg, kör agent, få resultat
- **När:** Multi-step uppgifter, externa system, komplex logik

### 💡 Nyckelpoäng
1. Agent = AI + Verktyg + Autonomi
2. Flera steg/iterationer automatiskt
3. Agenten väljer själv vilka verktyg den behöver
4. Perfekt för konsultmatchning, kundtjänst, dataanalys

### 🎯 Demo i TestConsole
**Menyval 1-3** visar agents i praktiken:
- Menyval 1: Math agent (Add, Multiply, Percentage)
- Menyval 2: Custom tools (Database, Weather)
- Menyval 3: Multi-step reasoning
- Menyval C: Konsultmatchning (ditt mål!)

### 📊 Jämförelse
| Aspekt | Function Call | Agent |
|--------|--------------|-------|
| Steg | 1 | Många |
| Verktyg | Nej | Ja |
| Beslut | Nej | Ja |
| Kostnad | Låg | Högre |
| Komplexitet | Låg | Hög |

## Vidare Läsning

**Opper Dokumentation:**
- Agent Framework: https://docs.opper.ai/agents/overview
- Tools & Functions: https://docs.opper.ai/agents/tools

**Externa Resurser:**
- LangChain Agents: https://python.langchain.com/docs/modules/agents/
- OpenAI Function Calling: https://platform.openai.com/docs/guides/function-calling

**Relaterade Koncept:**
- **ReAct**: Reasoning + Acting pattern
- **Chain of Thought**: AI tänker steg-för-steg
- **Tool Use**: AI använder externa verktyg
