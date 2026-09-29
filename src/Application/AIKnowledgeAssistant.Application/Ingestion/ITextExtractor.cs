namespace AIKnowledgeAssistant.Application.Ingestion;

public interface ITextExtractor
{
    bool CanExtract(string fileName);

    Task<string> ExtractAsync(Stream content, CancellationToken cancellationToken);
}
