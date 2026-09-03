using System.ClientModel;
using EmployeePolicyAssistant.Models;
using OpenAI;
using OpenAI.Chat;

namespace EmployeePolicyAssistant.Services;

public sealed class LlmService
{
    private readonly ChatClient _client;

    public LlmService(OpenAIOptions options)
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

        _client = client.GetChatClient(options.ChatModel);
    }

    public async Task<string> GenerateAnswerAsync(string context, string question, CancellationToken cancellationToken = default)
    {
        const string systemPrompt =
            "You are an AI assistant. Use the following context to answer the question. " +
            "If the answer is not in the context, say \"I don't know based on the given information.\" " +
            "Do not make up answers.";

        var userPrompt = $"Context: {context}\nQuestion: {question}\nAnswer:";

        List<ChatMessage> messages =
        [
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        ];

        ClientResult<ChatCompletion> result = await _client.CompleteChatAsync(messages, cancellationToken: cancellationToken);
        return result.Value.Content[0].Text;
    }
}
