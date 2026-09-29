using AIKnowledgeAssistant.Domain.Documents;

namespace AIKnowledgeAssistant.Application.Ingestion;

public interface IDocumentChunkRepository
{
    Task<IReadOnlyList<DocumentChunk>> ListByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken);

    Task ReplaceForDocumentAsync(
        Guid documentId,
        IReadOnlyList<DocumentChunk> chunks,
        CancellationToken cancellationToken);
}
