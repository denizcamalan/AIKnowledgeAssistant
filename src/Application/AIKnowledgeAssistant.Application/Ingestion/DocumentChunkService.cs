using AIKnowledgeAssistant.Application.Documents;
using AIKnowledgeAssistant.Domain.Documents;

namespace AIKnowledgeAssistant.Application.Ingestion;

public sealed record DocumentChunkResult(
    Guid Id,
    Guid DocumentId,
    int ChunkIndex,
    string Text,
    int StartOffset,
    int EndOffset);

public interface IDocumentChunkService
{
    Task<IReadOnlyList<DocumentChunkResult>> ListByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken);
}

public sealed class DocumentChunkService : IDocumentChunkService
{
    private readonly IDocumentRepository _documents;
    private readonly IDocumentChunkRepository _chunks;

    public DocumentChunkService(IDocumentRepository documents, IDocumentChunkRepository chunks)
    {
        _documents = documents;
        _chunks = chunks;
    }

    public async Task<IReadOnlyList<DocumentChunkResult>> ListByDocumentIdAsync(
        Guid documentId,
        CancellationToken cancellationToken)
    {
        if (await _documents.GetByIdAsync(documentId, cancellationToken) is null)
        {
            throw new DocumentNotFoundException(documentId);
        }

        var chunks = await _chunks.ListByDocumentIdAsync(documentId, cancellationToken);
        return chunks
            .Select(c => new DocumentChunkResult(
                c.Id,
                c.DocumentId,
                c.ChunkIndex,
                c.Text,
                c.StartOffset,
                c.EndOffset))
            .ToList();
    }
}
