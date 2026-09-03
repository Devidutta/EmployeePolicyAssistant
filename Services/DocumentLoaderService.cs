using System.Text;

namespace EmployeePolicyAssistant.Services;

public static class DocumentLoaderService
{
    public sealed record RawChunk(string Title, string Content);

    /// <summary>
    /// Splits a policy document into section-based chunks. Each section starts
    /// with a "## Title" heading line, followed by its content until the next
    /// heading.
    /// </summary>
    public static List<RawChunk> LoadAndSplit(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Policy file not found: {filePath}");
        }

        var chunks = new List<RawChunk>();
        string? currentTitle = null;
        var currentContent = new StringBuilder();

        foreach (var line in File.ReadLines(filePath))
        {
            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                FlushChunk(chunks, currentTitle, currentContent);
                currentTitle = line[3..].Trim();
                currentContent.Clear();
            }
            else
            {
                currentContent.AppendLine(line);
            }
        }

        FlushChunk(chunks, currentTitle, currentContent);

        return chunks;
    }

    private static void FlushChunk(List<RawChunk> chunks, string? title, StringBuilder content)
    {
        if (title is null)
        {
            return;
        }

        var text = content.ToString().Trim();
        if (text.Length > 0)
        {
            chunks.Add(new RawChunk(title, text));
        }
    }
}
