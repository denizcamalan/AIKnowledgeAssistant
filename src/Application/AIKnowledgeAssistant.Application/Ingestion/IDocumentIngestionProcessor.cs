using AIKnowledgeAssistant.Domain.Documents;

namespace AIKnowledgeAssistant.Application.Ingestion;

/// <summary>
/// Extract → chunk → embed steps land in later tasks; P5-01 only defines the hook.
/// </summary>
public interface IDocumentIngestionProcessor
{
    Task ProcessAsync(Document document, CancellationToken cancellationToken);
}
