using OpperSharp.Core;
using OpperSharp.Models.Functions;
using static System.Console;

namespace OpperSharp.UI.TestConsole
{
	internal class Program
	{
		private readonly OpperClient _client;

		public Program()
		{
			_client = OpperClient.FromEnvironment();
		}

		static void Main(string[] args)
		{
			var program = new Program();
			while (true)
			{
				try
				{
					ShowMenu();

					switch (ReadKey().KeyChar)
					{
						case '1': program.AskQuestion().GetAwaiter().GetResult(); break;
						case '2': program.FunctionAPITest().GetAwaiter().GetResult(); break;
						case '3': program.FuntionsStreaming().GetAwaiter().GetResult(); break;
						case '4': program.RagPipeline().GetAwaiter().GetResult(); break;
						case '0': WriteLine(); return;

						default: ShowMenu(); break;
					}
				}
				catch (Exception ex)
				{
					WriteLine();
					WriteLine("There was an error: ");
					while (ex != null)
					{
						WriteLine(ex.Message);
						ex = ex.InnerException;
					}
				}

				WriteLine();
				WriteLine("Press <ENTER> to return to menu.");
				ReadLine();
			}
		}

		public static void ShowMenu()
		{
			Clear();
			WriteLine("PREREQ:");
			WriteLine("1. Set first req here,");
			WriteLine("2. Set second req here");
			WriteLine("");
			WriteLine("- Select DecryptionService command.");
			WriteLine("");
			WriteLine(" 1) Ställ en fråga.");
			WriteLine(" 2) Testa Functions API.");
			WriteLine(" 3) Testa Functions Streaming.");
			WriteLine(" 4) Testa en RAG-pipeline.");
			WriteLine(" 0) Exit");

			WriteLine("");
			Write(" > ");
		}

		private async Task AskQuestion()
		{
			WriteLine("Ange din fråga:");
			var userPrompt = ReadLine() ?? string.Empty;

			var response = await _client.CallAsync(
				path: "func",
				input: new Dictionary<string, object>
				{
					["question"] = $"{userPrompt}" // "Vad är AI?"
				});
		}

		private async Task FunctionAPITest()
		{
			var documentText = File.ReadAllText(@"C:\Temp\Verama - Why I will succeed.txt");

			var response = await _client.CallAsync(
				path: "document-summarizer",
				input: new Dictionary<string, object>
				{
					["document"] = documentText
				},
				options: new OpperCallOptions
				{
					Model = "gpt-4",
					Temperature = 0.3,
					Cache = true
				}
			);

			WriteLine(response.Message);
			WriteLine($"Cachad: {response.Cached}");
		}

		private async Task FuntionsStreaming()
		{
			WriteLine("Ange din fråga:");
			var userPrompt = ReadLine() ?? string.Empty;

			Write("AI: ");

			await foreach (var chunk in _client.CallStreamAsync(
				path: "story-generator",
				input: new Dictionary<string, object>
				{
					["prompt"] = $"{userPrompt}" // "Skriv en kort berättelse om AI"
				}
			))
			{
				if (chunk.Delta != null)
				{
					Write(chunk.Delta);
				}
			}
		}

		private async Task RagPipeline()
		{
			WriteLine("Ange din fråga:");
			var userPrompt = ReadLine() ?? string.Empty; 

			// Steg 1: Sök kunskapsbas
			var searchResults = await _client.Indexes.QueryAsync(
				indexName: "företagsdokument",
				query: $"{userPrompt}?", // "Vad är vår kodgranskningsprocess?",
				k: 3
			);

			// Steg 2: Bygg kontext
			var context = string.Join("\n\n",
				searchResults.Results.Select(r => r.Content));

			// Steg 3: Generera svar med kontext
			var answer = await _client.Chat.CompleteAsync(
				prompt: userPrompt, // Vad är vår kodgranskningsprocess?
				systemPrompt: $@"Svara endast med denna kontext: {context}. 
				Om inte i kontext, säg 'Jag har inte den informationen.'"
			);

			WriteLine(answer);
		}
	}
}
