namespace EmployeePolicyAssistant.Models;

public class RagOptions
{
    public int TopK { get; init; } = 3;
    public required string PolicyFilePath { get; init; }
}
