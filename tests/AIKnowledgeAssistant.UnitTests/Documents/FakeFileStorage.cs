using AIKnowledgeAssistant.Application.Documents;
using AIKnowledgeAssistant.Application.Documents.StructuredOutput;

namespace AIKnowledgeAssistant.UnitTests.Documents;

internal sealed class FakeFileStorage : IFileStorage
{
    private readonly Dictionary<string, byte[]> _files = new(StringComparer.Ordinal);

    public List<string> DeletedPaths { get; } = [];

    public Task<string> SaveAsync(
        Guid documentId,
        Stream content,
        string fileName,
        CancellationToken cancellationToken)
    {
        using var memory = new MemoryStream();
        content.CopyTo(memory);
        var path = $"{documentId:N}/{fileName}";
        _files[path] = memory.ToArray();
        return Task.FromResult(path);
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken)
    {
        DeletedPaths.Add(storagePath);
        _files.Remove(storagePath);
        return Task.CompletedTask;
    }

    public Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken)
    {
        if (!_files.TryGetValue(storagePath, out var bytes))
        {
            throw new FileNotFoundException(storagePath);
        }

        return Task.FromResult<Stream>(new MemoryStream(bytes, writable: false));
    }
}
