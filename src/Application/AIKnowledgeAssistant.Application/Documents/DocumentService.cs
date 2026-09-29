using AIKnowledgeAssistant.Application.Configuration;
using AIKnowledgeAssistant.Domain.Documents;
using Microsoft.Extensions.Options;

namespace AIKnowledgeAssistant.Application.Documents;

public sealed class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _repository;
    private readonly IFileStorage _fileStorage;
    private readonly DocumentStorageOptions _storageOptions;

    public DocumentService(
        IDocumentRepository repository,
        IFileStorage fileStorage,
        IOptions<DocumentStorageOptions> storageOptions)
    {
        _repository = repository;
        _fileStorage = fileStorage;
        _storageOptions = storageOptions.Value;
    }

    public async Task<DocumentDetailResult> UploadAsync(
        Stream content,
        string originalFileName,
        string contentType,
        long sizeBytes,
        string? displayName,
        CancellationToken cancellationToken)
    {
        ValidateUpload(originalFileName, sizeBytes);

        if (await _repository.ExistsByOriginalFileNameAsync(originalFileName, cancellationToken))
        {
            throw new DuplicateDocumentException(originalFileName);
        }

        var now = DateTimeOffset.UtcNow;
        var document = new Document
        {
            Id = Guid.NewGuid(),
            OriginalFileName = originalFileName,
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? originalFileName : displayName.Trim(),
            ContentType = contentType,
            SizeBytes = sizeBytes,
            Status = DocumentStatus.Uploaded,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        document.StoragePath = await _fileStorage.SaveAsync(document.Id, content, originalFileName, cancellationToken);

        await _repository.AddAsync(document, cancellationToken);

        return DocumentMapping.ToDetail(document);
    }

    public async Task<IReadOnlyList<DocumentSummaryResult>> ListAsync(CancellationToken cancellationToken)
    {
        var documents = await _repository.ListAsync(cancellationToken);
        return documents
            .OrderByDescending(d => d.CreatedAtUtc)
            .Select(DocumentMapping.ToSummary)
            .ToList();
    }

    public async Task<DocumentDetailResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await _repository.GetByIdAsync(id, cancellationToken);
        if (document is null)
        {
            throw new DocumentNotFoundException(id);
        }

        return DocumentMapping.ToDetail(document);
    }

    public async Task<DocumentDetailResult> UpdateAsync(Guid id, string displayName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Display name is required.", nameof(displayName));
        }

        var document = await _repository.GetByIdAsync(id, cancellationToken);
        if (document is null)
        {
            throw new DocumentNotFoundException(id);
        }

        document.DisplayName = displayName.Trim();
        document.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await _repository.UpdateAsync(document, cancellationToken);

        return DocumentMapping.ToDetail(document);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await _repository.GetByIdAsync(id, cancellationToken);
        if (document is null)
        {
            throw new DocumentNotFoundException(id);
        }

        // Transaction boundary: one SaveChangesAsync per repository call (implicit DB transaction).
        // File delete runs after the row is removed; a failed file delete may leave an orphan on disk (logged in P1-05).
        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            throw new DocumentNotFoundException(id);
        }

        await _fileStorage.DeleteAsync(document.StoragePath, cancellationToken);
    }

    private void ValidateUpload(string originalFileName, long sizeBytes)
    {
        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            throw new ArgumentException("File name is required.", nameof(originalFileName));
        }

        if (sizeBytes <= 0)
        {
            throw new ArgumentException("File is empty.", nameof(sizeBytes));
        }

        if (sizeBytes > _storageOptions.MaxFileSizeBytes)
        {
            throw new ArgumentException(
                $"File exceeds the maximum size of {_storageOptions.MaxFileSizeBytes} bytes.",
                nameof(sizeBytes));
        }

        var extension = Path.GetExtension(originalFileName);
        if (!_storageOptions.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"File type '{extension}' is not allowed. Allowed: {string.Join(", ", _storageOptions.AllowedExtensions)}.",
                nameof(originalFileName));
        }
    }
}
