using AIKnowledgeAssistant.Application.Documents;
using AIKnowledgeAssistant.Domain.Documents;
using Microsoft.EntityFrameworkCore;

namespace AIKnowledgeAssistant.Infrastructure.Persistence;

internal sealed class EfDocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _dbContext;

    public EfDocumentRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await _dbContext.Documents.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Document>> ListAsync(CancellationToken cancellationToken) =>
        await _dbContext.Documents.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<bool> ExistsByOriginalFileNameAsync(string originalFileName, CancellationToken cancellationToken) =>
        await _dbContext.Documents.AnyAsync(
            d => d.OriginalFileName.ToLower() == originalFileName.ToLower(),
            cancellationToken);

    public async Task AddAsync(Document document, CancellationToken cancellationToken)
    {
        _dbContext.Documents.Add(document);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Document document, CancellationToken cancellationToken)
    {
        _dbContext.Documents.Update(document);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await _dbContext.Documents.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (document is null)
        {
            return false;
        }

        _dbContext.Documents.Remove(document);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
