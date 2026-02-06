using OpperSharp.Core;
using System;
using System.Threading.Tasks;

// QUICK TEST - Kör detta för att testa API-anslutningen
// Kompilera och kör: dotnet run

class Program
{
    static async Task Main()
    {
        try
        {
            Console.WriteLine("Initializing OpperClient...");
            var client = OpperClient.FromEnvironment();
            Console.WriteLine("✓ Client initialized\n");

            // Test 1: Embeddings (kräver INTE functions)
            Console.WriteLine("TEST 1: Embeddings API");
            Console.WriteLine("=======================");
            try
            {
                var embedding = await client.Embeddings.CreateAsync(
                    "Hello world",
                    "azure/text-embedding-3-small"
                );
                Console.WriteLine($"✓ SUCCESS! Got vector with {embedding.Count} dimensions");
                Console.WriteLine($"First 3: [{embedding[0]:F4}, {embedding[1]:F4}, {embedding[2]:F4}]\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ FAILED: {ex.Message}\n");
            }

            // Test 2: List Functions
            Console.WriteLine("TEST 2: List Functions");
            Console.WriteLine("=======================");
            try
            {
                var functions = await client.Functions.ListAsync();
                Console.WriteLine($"✓ SUCCESS! Found {functions.Count} functions:");
                if (functions.Count == 0)
                {
                    Console.WriteLine("(No functions created yet - create them in Opper Dashboard)");
                }
                else
                {
                    foreach (var func in functions)
                    {
                        Console.WriteLine($"  - {func.Path}");
                    }
                }
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ FAILED: {ex.Message}\n");
            }

            Console.WriteLine("\nDone! Press any key to exit...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
        }
    }
}
