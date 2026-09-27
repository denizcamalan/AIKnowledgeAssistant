using AIKnowledgeAssistant.Domain.Documents;

namespace AIKnowledgeAssistant.Application.Documents;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Document>> ListAsync(CancellationToken cancellationToken);

    Task<bool> ExistsByOriginalFileNameAsync(string originalFileName, CancellationToken cancellationToken);

    Task AddAsync(Document document, CancellationToken cancellationToken);

    Task UpdateAsync(Document document, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
