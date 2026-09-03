namespace EmployeePolicyAssistant.Models;

public sealed class VectorRecord
{
    public required string Title { get; init; }
    public required string Content { get; init; }
    public required ReadOnlyMemory<float> Vector { get; init; }
}
