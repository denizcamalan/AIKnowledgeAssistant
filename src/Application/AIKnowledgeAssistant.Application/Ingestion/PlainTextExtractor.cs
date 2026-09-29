using System.Text;

namespace AIKnowledgeAssistant.Application.Ingestion;

public sealed class PlainTextExtractor : ITextExtractor
{
    private static readonly string[] SupportedExtensions = [".txt", ".md"];

    public bool CanExtract(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        return SupportedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<string> ExtractAsync(Stream content, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(content, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        return await reader.ReadToEndAsync(cancellationToken);
    }
}
