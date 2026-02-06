# Hur man raderar Model Aliases manuellt i Opper Dashboard

## Steg-för-steg guide

### 1. Logga in på Opper Dashboard
- Gå till: https://platform.opper.ai
- Logga in med ditt konto

### 2. Navigera till Models
- I sidomenyn, klicka på **"Models"** eller **"Model Aliases"**
- (Exakt namn kan variera beroende på Opper's UI-version)

### 3. Hitta aliaserna
Du kommer att se en lista med alla dina model aliases, t.ex.:
```
Namn                                  Fallback Models
----------------------------------------------------
test-reliable-claude-639058783...    anthropic/claude-opus-4.5, ...
test-reliable-claude-639058785...    anthropic/claude-opus-4.5, ...
production-gpt4                       openai/gpt-4o, ...
```

### 4. Radera alias
För varje alias du vill radera:

**Alternativ A: Via knapp/ikon**
- Klicka på **trash-ikonen** (🗑️) eller **"Delete"** vid aliasets rad
- Bekräfta raderingen i dialogen som dyker upp

**Alternativ B: Via alias-detaljer**
- Klicka på aliasets namn för att öppna detaljvyn
- Klicka på **"Delete Alias"** eller **"Remove"** knappen
- Bekräfta raderingen

### 5. Verifiera radering
- Aliaset ska försvinna från listan
- Om du kör TestConsole menyval 9 igen, ska det nu visa färre aliases i steg 2

## Tips

### Hitta alla test-aliases snabbt
Test-aliases från OpperSharp TestConsole har namn som börjar med:
- `test-reliable-claude-639...`
- `test-reliable-gpt4-639...`

Siffran efter namnet är ett timestamp (DateTime.Now.Ticks).

### Skydda produktions-aliases
Om du har riktiga produktions-aliases, se till att:
- **INTE** döpa dem med prefix `test-`
- Ge dem beskrivande namn som `production-gpt4`, `staging-claude`, etc.
- Då kommer TestConsole automatiska cleanup inte röra dem

### Om du inte hittar "Models" i menyn

Opper Dashboard kan ha olika layouts. Försök:
1. Sök efter "Aliases" i sökfältet
2. Kolla under "Settings" → "Model Configuration"
3. Kolla under "Advanced" eller "API"

### Radera via API (om Dashboard saknar funktionen)

Om du inte kan hitta knappen i Dashboard, kan du radera via OpperSharp:

```csharp
using OpperSharp.Core;

var client = OpperClient.FromEnvironment();

// 1. Lista alla aliases
var aliases = await client.Models.ListAliasesAsync();

// 2. Hitta den du vill radera
var aliasToDelete = aliases.FirstOrDefault(a => a.Name == "test-reliable-claude-639058783175615306");

// 3. Radera den
if (aliasToDelete != null)
{
    await client.Models.DeleteAliasAsync(aliasToDelete.Id);
    Console.WriteLine($"Deleted: {aliasToDelete.Name}");
}
```

### Radera alla test-aliases via kod

Om du har många gamla test-aliases:

```csharp
var client = OpperClient.FromEnvironment();
var aliases = await client.Models.ListAliasesAsync();

var testAliases = aliases.Where(a => a.Name.StartsWith("test-"));

foreach (var alias in testAliases)
{
    await client.Models.DeleteAliasAsync(alias.Id);
    Console.WriteLine($"Deleted: {alias.Name}");
}

Console.WriteLine($"Cleaned up {testAliases.Count()} test aliases");
```

## Felsökning

### "Delete"-knappen är grå/inaktiv
- Kolla om du har rätt behörigheter
- Vissa aliases kan vara "system aliases" som inte går att radera

### Får felmeddelande vid radering
- **"UnprocessableEntity"**: Alias används av en funktion - ta bort från funktionen först
- **"NotFound"**: Aliaset har redan raderats
- **"Forbidden"**: Du har inte behörighet att radera detta alias

### Aliaset kommer tillbaka
- Har du flera TestConsole-instanser som körs samtidigt?
- Kolla om någon annan i teamet skapar aliases automatiskt
- Kontrollera om du har kod som automatiskt skapar aliases vid uppstart

## Automatisk cleanup i TestConsole

Från och med den senaste versionen städar **menyval 9** automatiskt upp alla test-aliases som börjar med `test-reliable-`. Du behöver inte göra något manuellt!

**Output från menyval 9:**
```
3. Cleaning up all test aliases...
   Found 2 test alias(es) to delete
   ✓ Deleted: test-reliable-claude-639058783175615306
   ✓ Deleted: test-reliable-claude-639058785941022991
```

## Sammanfattning

**Manuell radering i Dashboard:**
1. Gå till https://platform.opper.ai
2. Klicka på "Models" eller "Model Aliases"
3. Klicka på trash-ikonen (🗑️) vid aliaset
4. Bekräfta

**Automatisk radering via TestConsole:**
- Kör bara **menyval 9** - det städar automatiskt!

**Programmatisk radering:**
```csharp
var client = OpperClient.FromEnvironment();
var aliases = await client.Models.ListAliasesAsync();
var toDelete = aliases.First(a => a.Name == "mitt-alias");
await client.Models.DeleteAliasAsync(toDelete.Id);
```
