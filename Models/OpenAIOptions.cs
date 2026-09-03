namespace EmployeePolicyAssistant.Models;

public class OpenAIOptions
{
    public required string ApiKey { get; init; }
    public string? Endpoint { get; init; }
    public required string EmbeddingModel { get; init; }
    public required string ChatModel { get; init; }
}
