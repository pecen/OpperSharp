# Embeddings - Förklaring

## Vad är Embeddings?

**Kort svar:** Embeddings är en matematisk representation av text som datorer kan förstå och jämföra.

**Längre förklaring:**
När en AI behöver förstå text, måste den först konvertera texten till siffror. Embeddings är en teknik som tar en text (t.ex. en mening eller ett stycke) och konverterar den till en lång lista med decimaltal - en så kallad "vektor".

## Praktiskt Exempel

**Text:** "Hund är ett husdjur"
**Embedding:** [0.234, -0.156, 0.892, 0.445, ..., -0.234]
(3072 tal i Opper's fall med modellen `text-embedding-3-large`)

## Varför är detta användbart?

### 1. **Semantisk Likhet**
Texter med liknande *betydelse* får liknande embeddings, även om orden är olika:
- "Hund är ett husdjur" → vektor A
- "En hund är ett sällskapsdjur" → vektor B
- "Katt är ett husdjur" → vektor C

Vektor A och B är mer lika varandra än vektor C, eftersom de handlar om samma sak (hundar).

### 2. **Matematiska Beräkningar**
Med embeddings kan vi beräkna *hur lika* två texter är genom att använda matematik (cosine similarity):
- 1.0 = identiska
- 0.5 = lite lika
- 0.0 = helt olika
- -1.0 = motsatser

## Användningsområden

### 1. **Semantisk Sökning**
Hitta dokument baserat på *betydelse*, inte bara exakta ord:
```
Sökfråga: "anställningskontrakt"
Resultat:
- ✓ "Arbetsavtal mellan företag och medarbetare"
- ✓ "Tjänsteavtal och anställningsvillkor"
- ✗ "Recept på chokladkaka"
```

### 2. **Dokumentklassificering**
Gruppera liknande dokument automatiskt:
- Kundklagomål vs. Beröm
- Tekniska frågor vs. Administrativa frågor

### 3. **Rekommendationssystem**
"Kunder som läste denna artikel läste också..."

### 4. **RAG (Retrieval-Augmented Generation)**
Hitta relevant information innan AI svarar:
1. Användare: "Vad är vår policy om semester?"
2. System: Skapar embedding av frågan
3. System: Hittar liknande embeddings i dokumentdatabasen
4. System: Skickar relevanta dokument till AI
5. AI: Svarar baserat på faktisk policy

## I OpperSharp TestConsole

I **menyval 8** ser du embeddings i praktiken:

```
Text 1: "Machine learning is a subset of artificial intelligence"
Text 2: "Neural networks are inspired by biological neurons"
Text 3: "Python is a popular programming language"
```

**Resultat:**
- Text 1 och 2: Likhet = 0.3441 (ganska lika, båda om AI/ML)
- Text 1 och 3: Likhet = lägre (AI vs. programmeringsspråk)

## Tekniska Detaljer

### Dimensioner
Vektorn från Opper's modell har **3072 dimensioner** (3072 tal).
- Fler dimensioner = mer nyanserad representation
- Men också mer beräkningsintensivt

### Modeller i Opper
- `azure/text-embedding-3-small` - Snabbare, mindre exakt
- `azure/text-embedding-3-large` - Långsammare, mer exakt (3072 dim)

### Cosine Similarity
Så här beräknas likhet mellan två vektorer:

```
similarity = (A · B) / (||A|| × ||B||)
```

Där:
- A · B = dot product (skalärprodukt)
- ||A|| = längden av vektor A
- Resultat mellan -1 och 1

## Konsultmatchning - Praktiskt Exempel

I din **Consultant Matching** use case kan embeddings användas så här:

### 1. Skapa Embeddings för Konsultprofiler
```
Konsult: "Johan - Senior .NET utvecklare med 10 års erfarenhet av C#,
Azure, microservices och agil utveckling. Specialist på finanssystem."

Embedding: [0.234, -0.156, 0.892, ..., -0.234]
```

### 2. Skapa Embedding för Uppdragsbeskrivning
```
Uppdrag: "Vi söker en erfaren backend-utvecklare för att bygga ett
nytt betalningssystem i molnet. Krav: C#, Azure, erfarenhet av
finansiella system."

Embedding: [0.221, -0.149, 0.901, ..., -0.229]
```

### 3. Beräkna Likhet
```
Similarity(Johan, Uppdrag) = 0.85  ← Mycket bra match!
```

### 4. Rankning
Sortera alla konsulter efter similarity-score för att hitta bästa matchningen.

## Fördelar med Embeddings för Konsultmatchning

✅ **Semantisk förståelse**
- Förstår att "backend-utvecklare" ≈ ".NET utvecklare"
- Förstår att "molnet" ≈ "Azure"

✅ **Flexibel matchning**
- Hittar bra matchningar även om exakta ord inte matchar

✅ **Skalbart**
- Jämför 1000-tals konsulter på millisekunder

✅ **Multi-faktor**
- En vektor kan representera kompetens, erfarenhet, bransch, etc.

## Begränsningar

⚠️ **Inte perfekt för exakta krav**
- "Måste ha körkort" vs "Har körkort" → Liknande embeddings, men kritisk skillnad

⚠️ **Språkberoende**
- Modeller tränade på engelska fungerar sämre på svenska

⚠️ **Ingen logik**
- Förstår inte "INTE" eller komplex logik
- "Har 10 års erfarenhet" vs "Har 2 års erfarenhet" → Liknande embeddings

## Best Practices

### 1. Kombinera med Strukturerad Data
```csharp
var matchScore =
    0.6 × EmbeddingSimilarity(konsult, uppdrag) +
    0.2 × ExactSkillMatch(konsult, uppdrag) +
    0.2 × ExperienceYearsMatch(konsult, uppdrag);
```

### 2. Använd för Initial Filtrering
1. Använd embeddings för att hitta top 20 kandidater
2. Använd detaljerad logik för att välja top 3

### 3. Uppdatera Regelbundet
Om konsulten får nya kompetenser, skapa nya embeddings.

## Kod-exempel i OpperSharp

### Skapa Embedding för En Text
```csharp
var client = OpperClient.FromEnvironment();

var embedding = await client.Embeddings.CreateAsync(
    text: "Senior C# utvecklare med Azure-erfarenhet",
    model: "azure/text-embedding-3-large"
);

Console.WriteLine($"Vektor med {embedding.Count} dimensioner");
```

### Skapa Embeddings för Flera Texter (Batch)
```csharp
var texts = new[]
{
    "Konsult 1 profil...",
    "Konsult 2 profil...",
    "Konsult 3 profil..."
};

var response = await client.Embeddings.CreateBatchAsync(
    texts,
    model: "azure/text-embedding-3-large"
);

foreach (var item in response.Data)
{
    Console.WriteLine($"Embedding: {item.Embedding.Count} dimensioner");
}
```

### Beräkna Likhet
```csharp
public double CosineSimilarity(List<double> a, List<double> b)
{
    var dotProduct = a.Zip(b, (x, y) => x * y).Sum();
    var magnitudeA = Math.Sqrt(a.Sum(x => x * x));
    var magnitudeB = Math.Sqrt(b.Sum(x => x * x));

    return dotProduct / (magnitudeA * magnitudeB);
}

// Användning
var similarity = CosineSimilarity(embeddingKonsult, embeddingUppdrag);
Console.WriteLine($"Match score: {similarity:P0}"); // Ex: 85%
```

## Sammanfattning för Presentation

### ⚡ Snabba Fakta
- **Vad:** Konverterar text till siffror (vektorer)
- **Varför:** Så datorer kan förstå och jämföra text semantiskt
- **Hur:** Använd Opper's Embeddings API
- **När:** Sökning, matchning, klassificering, RAG

### 💡 Nyckelpoäng
1. Embeddings = Matematisk representation av text
2. Liknande betydelse → Liknande vektorer
3. Perfekt för konsultmatchning
4. Kombinera med strukturerad data för bästa resultat

### 🎯 Demo i TestConsole
**Menyval 8** visar embeddings i praktiken:
- Genererar vektorer för 3 texter
- Visar dimensioner (3072 tal)
- Beräknar likhet mellan texter
- Perfekt för att förstå konceptet!

## Vidare Läsning

### Opper Dokumentation
- Embeddings API: https://docs.opper.ai/api-reference/embeddings/overview

### Externa Resurser
- OpenAI Embeddings Guide: https://platform.openai.com/docs/guides/embeddings
- Vector Databases: Pinecone, Weaviate, Qdrant

### Relaterade Koncept
- **Vector Databases:** Specialiserade databaser för att lagra och söka i embeddings
- **RAG (Retrieval-Augmented Generation):** Kombinera embeddings med LLMs
- **Semantic Search:** Sökning baserad på betydelse istället för nyckelord
