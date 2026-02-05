using OpperSharp.Core;
using System;
using System.Threading.Tasks;

// QUICK DEBUG TEST - Lista alla functions i ditt Opper-konto

class Program
{
    static async Task Main()
    {
        try
        {
            Console.WriteLine("Connecting to Opper...");
            var client = OpperClient.FromEnvironment();
            Console.WriteLine("✓ Connected\n");

            Console.WriteLine("Listing all functions in your Opper account:");
            Console.WriteLine("==============================================");

            var functions = await client.Functions.ListAsync();

            if (functions.Count == 0)
            {
                Console.WriteLine("❌ NO FUNCTIONS FOUND!");
                Console.WriteLine("\nThis means the functions were not created successfully in Opper Dashboard.");
                Console.WriteLine("Please go back to https://platform.opper.ai and verify:");
                Console.WriteLine("1. Functions section exists");
                Console.WriteLine("2. You created the 7 functions");
                Console.WriteLine("3. Functions were saved successfully");
            }
            else
            {
                Console.WriteLine($"✓ Found {functions.Count} function(s):\n");

                var expectedFunctions = new[]
                {
                    "math-solver",
                    "research-agent",
                    "problem-solver",
                    "general-qa",
                    "story-generator",
                    "chat-assistant",
                    "consultant-matcher"
                };

                foreach (var func in functions)
                {
                    var isExpected = Array.Exists(expectedFunctions, f => f == func.Path);
                    var marker = isExpected ? "✓" : " ";
                    Console.WriteLine($"{marker} Path: {func.Path}");
                    Console.WriteLine($"  ID: {func.Id}");
                    if (!string.IsNullOrEmpty(func.Description))
                        Console.WriteLine($"  Description: {func.Description}");
                    Console.WriteLine();
                }

                Console.WriteLine("\nExpected functions for TestConsole:");
                foreach (var expected in expectedFunctions)
                {
                    var found = functions.Exists(f => f.Path == expected);
                    var marker = found ? "✓" : "❌";
                    Console.WriteLine($"{marker} {expected}");
                }
            }

            Console.WriteLine("\n==============================================");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
        }
    }
}
