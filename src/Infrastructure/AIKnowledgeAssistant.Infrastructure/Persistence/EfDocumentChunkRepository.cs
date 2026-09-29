using AIKnowledgeAssistant.Application.Ingestion;
using AIKnowledgeAssistant.Domain.Documents;
using Microsoft.EntityFrameworkCore;

namespace AIKnowledgeAssistant.Infrastructure.Persistence;

internal sealed class EfDocumentChunkRepository : IDocumentChunkRepository
{
    private readonly AppDbContext _dbContext;

    public EfDocumentChunkRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public async Task<IReadOnlyList<DocumentChunk>> ListByDocumentIdAsync(
        Guid documentId,
        CancellationToken cancellationToken) =>
        await _dbContext.DocumentChunks
            .AsNoTracking()
            .Where(c => c.DocumentId == documentId)
            .OrderBy(c => c.ChunkIndex)
            .ToListAsync(cancellationToken);

    public async Task ReplaceForDocumentAsync(
        Guid documentId,
        IReadOnlyList<DocumentChunk> chunks,
        CancellationToken cancellationToken)
    {
        var existing = await _dbContext.DocumentChunks
            .Where(c => c.DocumentId == documentId)
            .ToListAsync(cancellationToken);

        if (existing.Count > 0)
        {
            _dbContext.DocumentChunks.RemoveRange(existing);
        }

        if (chunks.Count > 0)
        {
            _dbContext.DocumentChunks.AddRange(chunks);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
