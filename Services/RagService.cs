using EmployeePolicyAssistant.Models;

namespace EmployeePolicyAssistant.Services;

public sealed record RagAnswer(string Answer, List<SearchResult> RetrievedChunks);

public sealed class RagService
{
    private readonly EmbeddingService _embeddingService;
    private readonly LlmService _llmService;
    private readonly int _topK;
    private readonly List<VectorRecord> _index = [];

    public RagService(EmbeddingService embeddingService, LlmService llmService, int topK)
    {
        _embeddingService = embeddingService;
        _llmService = llmService;
        _topK = topK;
    }

    public async Task IndexAsync(string policyFilePath, CancellationToken cancellationToken = default)
    {
        var rawChunks = DocumentLoaderService.LoadAndSplit(policyFilePath);

        foreach (var chunk in rawChunks)
        {
            var vector = await _embeddingService.GetEmbeddingAsync(chunk.Content, cancellationToken);
            _index.Add(new VectorRecord
            {
                Title = chunk.Title,
                Content = chunk.Content,
                Vector = vector
            });
        }
    }

    public IReadOnlyList<VectorRecord> IndexedChunks => _index;

    public async Task<RagAnswer> AskAsync(string question, CancellationToken cancellationToken = default)
    {
        // Retrieval: embed the question and rank policy chunks by cosine similarity.
        var queryVector = await _embeddingService.GetEmbeddingAsync(question, cancellationToken);

        var ranked = _index
            .Select(record => new SearchResult(
                record.Title,
                record.Content,
                SimilarityService.CosineSimilarity(record.Vector.Span, queryVector.Span)))
            .OrderByDescending(r => r.Similarity)
            .Take(_topK)
            .ToList();

        // Augmentation: combine the retrieved chunks into a single context block.
        var context = string.Join(
            "\n\n",
            ranked.Select(r => $"[{r.Title}]\n{r.Content}"));

        // Generation: ask the LLM to answer using only the augmented context.
        var answer = await _llmService.GenerateAnswerAsync(context, question, cancellationToken);

        return new RagAnswer(answer, ranked);
    }
}
