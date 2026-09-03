using System.ClientModel;
using EmployeePolicyAssistant.Models;
using OpenAI;
using OpenAI.Embeddings;

namespace EmployeePolicyAssistant.Services;

public sealed class EmbeddingService
{
    private readonly EmbeddingClient _client;

    public EmbeddingService(OpenAIOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            throw new InvalidOperationException(
                "OpenAI:ApiKey is not set. Add it to appsettings.Local.json.");
        }

        OpenAIClient client = new(
            new ApiKeyCredential(options.ApiKey),
            new OpenAIClientOptions
            {
                Endpoint = string.IsNullOrWhiteSpace(options.Endpoint) ? null : new Uri(options.Endpoint)
            });

        _client = client.GetEmbeddingClient(options.EmbeddingModel);
    }

    public async Task<ReadOnlyMemory<float>> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        ClientResult<OpenAIEmbedding> result = await _client.GenerateEmbeddingAsync(text, cancellationToken: cancellationToken);
        return result.Value.ToFloats();
    }
}
