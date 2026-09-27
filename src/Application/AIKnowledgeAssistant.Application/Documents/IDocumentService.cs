namespace AIKnowledgeAssistant.Application.Documents;

public interface IDocumentService
{
    Task<DocumentDetailResult> UploadAsync(
        Stream content,
        string originalFileName,
        string contentType,
        long sizeBytes,
        string? displayName,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<DocumentSummaryResult>> ListAsync(CancellationToken cancellationToken);

    Task<DocumentDetailResult> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<DocumentDetailResult> UpdateAsync(Guid id, string displayName, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
