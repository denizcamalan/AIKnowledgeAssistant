using AIKnowledgeAssistant.Application.Documents;
using AIKnowledgeAssistant.Domain.Documents;

namespace AIKnowledgeAssistant.UnitTests.Documents;

internal sealed class FakeDocumentRepository : IDocumentRepository
{
    private readonly Dictionary<Guid, Document> _documents = new();

    public Document? Document { get; set; }

    public IReadOnlyCollection<Document> Documents => _documents.Values;

    public Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        if (Document?.Id == id)
        {
            return Task.FromResult<Document?>(Document);
        }

        return Task.FromResult(_documents.GetValueOrDefault(id));
    }

    public Task<IReadOnlyList<Document>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Document>>(_documents.Values.ToList());

    public Task<bool> ExistsByOriginalFileNameAsync(string originalFileName, CancellationToken cancellationToken) =>
        Task.FromResult(_documents.Values.Any(d =>
            string.Equals(d.OriginalFileName, originalFileName, StringComparison.Ordinal)));

    public Task AddAsync(Document document, CancellationToken cancellationToken)
    {
        _documents[document.Id] = document;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Document document, CancellationToken cancellationToken)
    {
        _documents[document.Id] = document;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_documents.Remove(id));
}
