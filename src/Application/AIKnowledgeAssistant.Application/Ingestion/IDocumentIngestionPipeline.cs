using AIKnowledgeAssistant.Application.Documents;

namespace AIKnowledgeAssistant.Application.Ingestion;

public interface IDocumentIngestionPipeline
{
    Task<DocumentDetailResult> RunAsync(Guid documentId, CancellationToken cancellationToken);
}
