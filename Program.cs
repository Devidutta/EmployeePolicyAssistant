using EmployeePolicyAssistant.Models;
using EmployeePolicyAssistant.Services;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.Local.json", optional: true)
    .Build();

var openAiOptions = configuration.GetSection("OpenAI").Get<OpenAIOptions>()
    ?? throw new InvalidOperationException("Missing 'OpenAI' configuration section.");

var ragOptions = configuration.GetSection("Rag").Get<RagOptions>()
    ?? throw new InvalidOperationException("Missing 'Rag' configuration section.");

var embeddingService = new EmbeddingService(openAiOptions);
var llmService = new LlmService(openAiOptions);
var ragService = new RagService(embeddingService, llmService, ragOptions.TopK);

var policyFilePath = Path.Combine(AppContext.BaseDirectory, ragOptions.PolicyFilePath);

Console.WriteLine("Indexing employee policy document...");
await ragService.IndexAsync(policyFilePath);
Console.WriteLine($"Indexed {ragService.IndexedChunks.Count} policy sections: " +
    string.Join(", ", ragService.IndexedChunks.Select(c => c.Title)));
Console.WriteLine();

Console.WriteLine("Employee Policy Assistant — ask a question about company policy (or type 'exit' to quit).");
Console.WriteLine();

while (true)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("Question> ");
    var question = Console.ReadLine();
    Console.ResetColor();

    if (string.IsNullOrWhiteSpace(question) ||
        question.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    var result = await ragService.AskAsync(question);

    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("[Retrieval] Top matching policy sections:");
    foreach (var chunk in result.RetrievedChunks)
    {
        Console.WriteLine($"  - {chunk.Title,-22} similarity: {chunk.Similarity:F4}");
    }

    Console.WriteLine();
    Console.WriteLine("[Augmentation] Retrieved section content added to the LLM prompt as context.");

    Console.WriteLine();
    Console.ResetColor();
    Console.WriteLine("[Generation] Answer:");
    Console.WriteLine(result.Answer);
    Console.WriteLine();
}

Console.WriteLine("Goodbye.");
