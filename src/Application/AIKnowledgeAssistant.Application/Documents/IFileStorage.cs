namespace AIKnowledgeAssistant.Application.Documents;

public interface IFileStorage
{
    Task<string> SaveAsync(Guid documentId, Stream content, string fileName, CancellationToken cancellationToken);

    Task DeleteAsync(string storagePath, CancellationToken cancellationToken);

    Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken);
}
