using AIKnowledgeAssistant.Application.Ingestion;
using AIKnowledgeAssistant.Domain.Documents;

namespace AIKnowledgeAssistant.UnitTests.Ingestion;

internal sealed class FakeDocumentChunkRepository : IDocumentChunkRepository
{
    private readonly Dictionary<Guid, List<DocumentChunk>> _byDocument = new();

    public IReadOnlyList<DocumentChunk> GetChunks(Guid documentId) =>
        _byDocument.GetValueOrDefault(documentId) ?? [];

    public Task<IReadOnlyList<DocumentChunk>> ListByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<DocumentChunk>>(GetChunks(documentId).OrderBy(c => c.ChunkIndex).ToList());

    public Task ReplaceForDocumentAsync(
        Guid documentId,
        IReadOnlyList<DocumentChunk> chunks,
        CancellationToken cancellationToken)
    {
        _byDocument[documentId] = chunks.OrderBy(c => c.ChunkIndex).ToList();
        return Task.CompletedTask;
    }
}
