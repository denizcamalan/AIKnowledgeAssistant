using AIKnowledgeAssistant.Application.Configuration;
using AIKnowledgeAssistant.Application.Documents;
using AIKnowledgeAssistant.Domain.Documents;
using Microsoft.Extensions.Options;

namespace AIKnowledgeAssistant.UnitTests.Documents;

public sealed class DocumentServiceTests
{
    private static DocumentService CreateService(
        FakeDocumentRepository? repository = null,
        FakeFileStorage? fileStorage = null) =>
        new(
            repository ?? new FakeDocumentRepository(),
            fileStorage ?? new FakeFileStorage(),
            Options.Create(DefaultOptions()));

    private static DocumentStorageOptions DefaultOptions() =>
        new()
        {
            MaxFileSizeBytes = 1024,
            AllowedExtensions = [".txt", ".pdf"],
        };

    [Fact]
    public async Task UploadAsync_ValidFile_PersistsAndReturnsDetail()
    {
        var repository = new FakeDocumentRepository();
        var service = CreateService(repository);

        await using var stream = new MemoryStream("hello"u8.ToArray());
        var result = await service.UploadAsync(
            stream,
            "notes.txt",
            "text/plain",
            5,
            displayName: "My notes",
            CancellationToken.None);

        Assert.Equal("My notes", result.DisplayName);
        Assert.Equal("notes.txt", result.OriginalFileName);
        Assert.Single(repository.Documents);
        Assert.Equal(DocumentStatus.Uploaded, repository.Documents.First().Status);
    }

    [Fact]
    public async Task UploadAsync_DuplicateFileName_ThrowsDuplicateDocumentException()
    {
        var repository = new FakeDocumentRepository();
        var service = CreateService(repository);

        await using var first = new MemoryStream("a"u8.ToArray());
        await service.UploadAsync(first, "dup.txt", "text/plain", 1, null, CancellationToken.None);

        await using var second = new MemoryStream("b"u8.ToArray());
        await Assert.ThrowsAsync<DuplicateDocumentException>(() =>
            service.UploadAsync(second, "dup.txt", "text/plain", 1, null, CancellationToken.None));
    }

    [Fact]
    public async Task UploadAsync_DisallowedExtension_ThrowsArgumentException()
    {
        var service = CreateService();

        await using var stream = new MemoryStream("x"u8.ToArray());
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.UploadAsync(stream, "virus.exe", "application/octet-stream", 1, null, CancellationToken.None));

        Assert.Equal("originalFileName", exception.ParamName);
    }

    [Fact]
    public async Task GetByIdAsync_MissingDocument_ThrowsDocumentNotFoundException()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<DocumentNotFoundException>(() =>
            service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_EmptyDisplayName_ThrowsArgumentException()
    {
        var repository = new FakeDocumentRepository();
        var id = Guid.NewGuid();
        await repository.AddAsync(
            new Document
            {
                Id = id,
                OriginalFileName = "a.txt",
                DisplayName = "a.txt",
                ContentType = "text/plain",
                SizeBytes = 1,
                Status = DocumentStatus.Uploaded,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                UpdatedAtUtc = DateTimeOffset.UtcNow,
            },
            CancellationToken.None);

        var service = CreateService(repository);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.UpdateAsync(id, "   ", CancellationToken.None));

        Assert.Equal("displayName", exception.ParamName);
    }

    [Fact]
    public async Task DeleteAsync_ExistingDocument_RemovesRowAndDeletesStorage()
    {
        var repository = new FakeDocumentRepository();
        var fileStorage = new FakeFileStorage();
        var id = Guid.NewGuid();
        var document = new Document
        {
            Id = id,
            OriginalFileName = "a.txt",
            DisplayName = "a.txt",
            ContentType = "text/plain",
            SizeBytes = 1,
            Status = DocumentStatus.Uploaded,
            StoragePath = $"{id:N}/a.txt",
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
        };
        await repository.AddAsync(document, CancellationToken.None);

        var service = CreateService(repository, fileStorage);
        await service.DeleteAsync(id, CancellationToken.None);

        Assert.Empty(repository.Documents);
        Assert.Equal([document.StoragePath], fileStorage.DeletedPaths);
    }
}
