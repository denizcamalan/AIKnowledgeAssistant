using AIKnowledgeAssistant.Application.Documents;

namespace AIKnowledgeAssistant.UnitTests.Documents;

internal sealed class FakeFileStorage : IFileStorage
{
    public List<string> DeletedPaths { get; } = [];

    public Task<string> SaveAsync(
        Guid documentId,
        Stream content,
        string originalFileName,
        CancellationToken cancellationToken) =>
        Task.FromResult($"{documentId:N}/{originalFileName}");

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken)
    {
        DeletedPaths.Add(storagePath);
        return Task.CompletedTask;
    }
}
