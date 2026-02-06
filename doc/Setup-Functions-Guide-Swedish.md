# Steg-för-steg Guide: Skapa Functions i Opper för TestConsole

## Översikt

För att köra **menyval 1-6 och C** i TestConsole behöver du skapa **7 functions** i Opper Dashboard.

**Tid:** ~15-20 minuter totalt (2-3 minuter per function)

---

## Steg 1: Logga in på Opper Dashboard

1. Öppna din webbläsare
2. Gå till: **https://platform.opper.ai**
3. Logga in med ditt Opper-konto
4. Du kommer till Dashboard-översikten

---

## Steg 2: Hitta Functions-sektionen

1. I vänstermenyn, leta efter **"Functions"** eller **"Indexes"**
   - Exakt namn kan variera beroende på UI-version
   - Kan också heta "AI Functions" eller "Call Functions"
2. Klicka på **"Functions"**
3. Du kommer att se en lista med dina existerande functions (om några)

---

## Steg 3: Skapa Function #1 - math-solver (Menyval 1)

### 3.1 Klicka på "Create Function" eller "New Function"
- Knappen finns vanligtvis högst upp till höger
- Kan vara en + symbol eller en "Create" knapp

### 3.2 Fyll i Function-formuläret

**Function Path/Name:**
```
math-solver
```

**Instructions/Prompt:**
```
You are a mathematical problem solver with access to various calculation tools.

You have access to the following tools:
- Add: Adds two numbers together
- Multiply: Multiplies two numbers
- Percentage: Calculates percentage of a value
- Divide: Divides first number by second number
- Subtract: Subtracts second number from first number

When given a math problem:
1. Break it down into steps
2. Use the appropriate tools to solve each step
3. Show your work clearly
4. Give the final answer

Example:
Problem: "What is 25% of 200, plus 30?"
Steps:
1. Calculate 25% of 200 using Percentage tool → 50
2. Add 50 + 30 using Add tool → 80
Answer: 80
```

**Model:**
- Välj: **anthropic/claude-opus-4.5** eller **anthropic/claude-sonnet-4**
- (GPT-4 fungerar också om du har tillgång till OpenAI-modeller)

**Description (valfritt):**
```
Math solver agent with calculation tools for TestConsole
```

### 3.3 Spara functionen
- Klicka på **"Create"** eller **"Save"**
- Du ska nu se functionen i listan

---

## Steg 4: Skapa Function #2 - research-agent (Menyval 2)

### Klicka på "Create Function" igen

**Function Path/Name:**
```
research-agent
```

**Instructions/Prompt:**
```
You are a research assistant with access to database and weather information.

You have access to the following tools:
- get_consultants: Retrieves consultant information from database
- get_weather: Gets current weather forecast for a city

Use these tools to answer user questions about:
- Consultant information, skills, and availability
- Weather conditions in different cities
- Any queries that require data lookup

Always cite which tool you used to get the information.
```

**Model:**
- Välj: **anthropic/claude-sonnet-4** eller **anthropic/claude-opus-4.5**

**Description:**
```
Research agent with database and weather tools
```

**Spara functionen**

---

## Steg 5: Skapa Function #3 - problem-solver (Menyval 3)

**Function Path/Name:**
```
problem-solver
```

**Instructions/Prompt:**
```
You are an expert problem solver that breaks down complex problems into steps.

Approach:
1. Analyze the problem carefully
2. Break it into smaller, manageable sub-problems
3. Use available tools to solve each sub-problem
4. Synthesize the results into a final answer
5. Explain your reasoning clearly

You have access to various tools that will be provided at runtime.
Use them wisely and explain why you chose each tool.

Always show your step-by-step thinking process.
```

**Model:**
- Välj: **anthropic/claude-opus-4.5** (bäst för reasoning)

**Description:**
```
Multi-step problem solver for complex tasks
```

**Spara functionen**

---

## Steg 6: Skapa Function #4 - general-qa (Menyval 4)

**Function Path/Name:**
```
general-qa
```

**Instructions/Prompt:**
```
You are a helpful AI assistant that answers questions accurately and concisely.

Guidelines:
- Provide clear, factual answers
- Be concise but thorough
- If you're not sure, say so
- Use examples when helpful
- Format your answers for readability

Respond in a friendly, professional tone.
```

**Model:**
- Välj: **anthropic/claude-sonnet-4**

**Description:**
```
General Q&A function for simple questions
```

**Spara functionen**

---

## Steg 7: Skapa Function #5 - story-generator (Menyval 5)

**Function Path/Name:**
```
story-generator
```

**Instructions/Prompt:**
```
You are a creative storyteller who generates engaging short stories.

When given a topic:
- Create a compelling 2-3 paragraph story
- Include interesting characters and plot
- Use vivid descriptions
- Make it engaging and entertaining
- Keep it appropriate for all audiences

Write in an engaging narrative style.
```

**Model:**
- Välj: **anthropic/claude-sonnet-4** eller **anthropic/claude-3.5-sonnet-20241022**

**Description:**
```
Creative story generator for TestConsole streaming demo
```

**Spara functionen**

---

## Steg 8: Skapa Function #6 - chat-assistant (Menyval 6)

**Function Path/Name:**
```
chat-assistant
```

**Instructions/Prompt:**
```
You are a helpful AI assistant for conversational chat.

Guidelines:
- Be friendly and concise
- Consider the conversation history when responding
- Ask clarifying questions when needed
- Provide helpful, relevant answers
- Maintain context throughout the conversation

The user will provide:
- "message": Their current message
- "history": Previous conversation (if any)

Respond naturally and conversationally.
```

**Model:**
- Välj: **anthropic/claude-sonnet-4**

**Description:**
```
Conversational chat assistant
```

**Spara functionen**

---

## Steg 9: Skapa Function #7 - consultant-matcher (Menyval C)

**Function Path/Name:**
```
consultant-matcher
```

**Instructions/Prompt:**
```
You are an expert consultant matching specialist.

Your task is to match consultants to assignments based on:
- Technical skills and competencies
- Years of experience
- Domain expertise
- Availability
- Hourly rate

You have access to tools:
- get_consultants: Retrieves all available consultants
- calculate_match_score: Calculates how well a consultant matches requirements

Process:
1. Analyze the assignment requirements carefully
2. Retrieve all consultants using get_consultants
3. For each relevant consultant, calculate match score
4. Compare the scores
5. Provide a ranked recommendation with clear reasoning

Format your response:
- Top recommendation with match percentage
- Key strengths that make this consultant a good fit
- Any potential concerns or gaps
- Alternative options if requested

Be thorough and justify your recommendations.
```

**Model:**
- Välj: **anthropic/claude-opus-4.5** (bäst för analys)

**Description:**
```
Consultant matching system for assignment recommendations
```

**Spara functionen**

---

## Steg 10: Verifiera alla Functions

### 10.1 Kontrollera listan
Du borde nu se **7 functions** i din lista:
1. ✅ math-solver
2. ✅ research-agent
3. ✅ problem-solver
4. ✅ general-qa
5. ✅ story-generator
6. ✅ chat-assistant
7. ✅ consultant-matcher

### 10.2 Testa att de fungerar
För varje function, klicka på namnet och du kan:
- Se function-detaljerna
- Testa den med en test-input (om UI tillåter)
- Redigera om något behöver ändras

---

## Steg 11: Testa i TestConsole

1. Öppna TestConsole
2. Kör programmet: `dotnet run`
3. Testa menyvalen:

```
Menyval 1: Agent: Basic Math
  - Borde använda math-solver function
  - Testar Add, Multiply, Percentage tools

Menyval 2: Agent: Custom Tools
  - Borde använda research-agent function
  - Testar get_consultants, get_weather tools

Menyval 3: Agent: Multi-step
  - Borde använda problem-solver function
  - Testar komplex problemlösning

Menyval 4: Simple Function Call
  - Borde använda general-qa function
  - Testar enkel Q&A

Menyval 5: Streaming Response
  - Borde använda story-generator function
  - Testar streaming av text

Menyval 6: Conversational Chat
  - Borde använda chat-assistant function
  - Testar konversation med historik

Menyval C: Consultant Matching
  - Borde använda consultant-matcher function
  - Testar hela matchningssystemet
```

---

## Troubleshooting

### Problem: "Function not found"

**Lösning:**
1. Kontrollera att function **path** är exakt rätt (case-sensitive!)
2. I Opper Dashboard, dubbelkolla att path = "math-solver" (inte "Math-Solver" eller "mathsolver")
3. Vänta 10-30 sekunder efter att ha skapat functionen (kan ta tid att propagera)

### Problem: "BadRequest" eller "Invalid request"

**Lösning:**
1. Kontrollera att du valde en giltig modell
2. Kolla att prompt:en inte är tom
3. Se till att function har sparats korrekt

### Problem: Agent ger konstiga svar

**Lösning:**
1. Kontrollera att prompt:en är tydlig
2. Använd en mer kraftfull modell (Opus istället för Haiku)
3. Lägg till fler exempel i prompt:en

### Problem: "Timeout" eller långsam respons

**Lösning:**
1. Agent-operationer kan ta 5-30 sekunder
2. Använd en snabbare modell (Sonnet eller Haiku)
3. Minska MaxIterations i AgentOptions

### Problem: "Insufficient permissions"

**Lösning:**
1. Kontrollera att din Opper API-nyckel har rätt behörigheter
2. Kolla att du är inloggad på rätt Opper-konto
3. Vissa organisationer kräver admin-rättigheter för att skapa functions

---

## Tips för Bästa Resultat

### 1. Välj Rätt Modell

**För snabba, enkla uppgifter:**
- `anthropic/claude-3.5-haiku` - Snabbast och billigast
- `anthropic/claude-sonnet-4` - Bra balans

**För komplex reasoning:**
- `anthropic/claude-opus-4.5` - Bäst kvalitet
- `anthropic/claude-opus-4` - Också mycket bra

**För OpenAI-modeller (om tillgängliga):**
- `openai/gpt-4o` - Bra allround
- `openai/gpt-4-turbo` - Snabbare variant

### 2. Skriv Tydliga Prompts

✅ **BRA:**
```
You are a math solver. Use the Add tool to add numbers.
Example: Add(2, 3) = 5
```

❌ **DÅLIGT:**
```
Do math stuff
```

### 3. Ge Exempel i Prompt

Agents fungerar bättre när de ser exempel:
```
Example input: "Calculate 15% of 200"
Example steps:
1. Use Percentage(200, 15) → 30
2. Return: "15% of 200 is 30"
```

### 4. Testa Stegvis

1. Skapa 1-2 functions först
2. Testa dem i TestConsole
3. När de fungerar, skapa resten
4. Finjustera prompts baserat på resultat

### 5. Använd Beskrivande Function Names

✅ Bra: `math-solver`, `consultant-matcher`, `chat-assistant`
❌ Dåligt: `func1`, `test`, `ai`

---

## Snabb-referens: Alla Functions

| # | Path | Menyval | Modell | Prompt-fokus |
|---|------|---------|--------|--------------|
| 1 | `math-solver` | 1 | Opus/Sonnet | Math tools, step-by-step |
| 2 | `research-agent` | 2 | Sonnet | Database & weather tools |
| 3 | `problem-solver` | 3 | Opus | Multi-step reasoning |
| 4 | `general-qa` | 4 | Sonnet | Concise Q&A |
| 5 | `story-generator` | 5 | Sonnet | Creative storytelling |
| 6 | `chat-assistant` | 6 | Sonnet | Conversational |
| 7 | `consultant-matcher` | C | Opus | Analysis & matching |

---

## Vanliga Frågor

**Q: Kostar det pengar att köra dessa functions?**
A: Ja, varje API-anrop kostar baserat på tokens och modell. Agent-operationer använder flera anrop (dyrare). Se Oppers prissättning.

**Q: Kan jag använda samma function för flera menyval?**
A: Tekniskt ja, men då fungerar inte TestConsole-demot optimalt. Varje function är designad för sitt specifika use case.

**Q: Måste jag använda exakt dessa model-namn?**
A: Nej, men använd modeller som finns i ditt Opper-konto (kör menyval 9 för att lista tillgängliga modeller).

**Q: Kan jag redigera functions efter att de skapats?**
A: Ja! Klicka på function-namnet i listan och välj "Edit" eller "Update".

**Q: Vad händer om jag glömmer en function?**
A: TestConsole kommer ge "Function not found"-fel. Skapa bara den saknade functionen och testa igen.

**Q: Kan jag radera test-functions senare?**
A: Ja, klicka på funktionen och välj "Delete". Eller använd OpperSharp:
```csharp
var functions = await client.Functions.ListAsync();
var toDelete = functions.First(f => f.Path == "math-solver");
await client.Functions.DeleteAsync(toDelete.Path);
```

---

## Nästa Steg

När alla 7 functions är skapade:
1. ✅ Kör TestConsole: `dotnet run`
2. ✅ Testa varje menyval (1-6 och C)
3. ✅ Observera hur agents använder tools
4. ✅ Experimentera med olika inputs
5. ✅ Anpassa prompts för bättre resultat

**Lycka till!** 🚀

Om du stöter på problem, kolla Troubleshooting-sektionen ovan eller Oppers dokumentation: https://docs.opper.ai
