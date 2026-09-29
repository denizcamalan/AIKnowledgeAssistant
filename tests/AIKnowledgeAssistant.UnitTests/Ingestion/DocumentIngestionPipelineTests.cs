using AIKnowledgeAssistant.Application.Documents;
using AIKnowledgeAssistant.Application.Ingestion;
using AIKnowledgeAssistant.Domain.Documents;
using AIKnowledgeAssistant.UnitTests.Documents;

namespace AIKnowledgeAssistant.UnitTests.Ingestion;

public sealed class DocumentIngestionPipelineTests
{
    [Fact]
    public async Task RunAsync_HappyPath_TransitionsToReady()
    {
        var repository = new FakeDocumentRepository();
        var id = Guid.NewGuid();
        await repository.AddAsync(
            new Document
            {
                Id = id,
                OriginalFileName = "notes.txt",
                DisplayName = "notes.txt",
                ContentType = "text/plain",
                SizeBytes = 5,
                Status = DocumentStatus.Uploaded,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                UpdatedAtUtc = DateTimeOffset.UtcNow,
            },
            CancellationToken.None);

        var pipeline = new DocumentIngestionPipeline(
            repository,
            new SuccessProcessor(),
            TimeProvider.System);

        var result = await pipeline.RunAsync(id, CancellationToken.None);

        Assert.Equal(DocumentStatus.Ready, result.Status);
        Assert.Equal(1, result.Ingestion.AttemptCount);
        Assert.NotNull(result.Ingestion.StartedAtUtc);
        Assert.NotNull(result.Ingestion.CompletedAtUtc);
        Assert.Null(result.Ingestion.FailureReason);
    }

    [Fact]
    public async Task RunAsync_ProcessorFails_TransitionsToFailedAndStoresReason()
    {
        var repository = new FakeDocumentRepository();
        var id = Guid.NewGuid();
        await repository.AddAsync(
            new Document
            {
                Id = id,
                OriginalFileName = "notes.txt",
                DisplayName = "notes.txt",
                ContentType = "text/plain",
                SizeBytes = 5,
                Status = DocumentStatus.Uploaded,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                UpdatedAtUtc = DateTimeOffset.UtcNow,
            },
            CancellationToken.None);

        var pipeline = new DocumentIngestionPipeline(
            repository,
            new FailingProcessor("boom"),
            TimeProvider.System);

        var result = await pipeline.RunAsync(id, CancellationToken.None);

        Assert.Equal(DocumentStatus.Failed, result.Status);
        Assert.Equal("boom", result.Ingestion.FailureReason);
        Assert.Equal(1, result.Ingestion.AttemptCount);
    }

    [Fact]
    public async Task RunAsync_FromFailed_IncrementsAttemptOnRetry()
    {
        var repository = new FakeDocumentRepository();
        var id = Guid.NewGuid();
        var document = new Document
        {
            Id = id,
            OriginalFileName = "notes.txt",
            DisplayName = "notes.txt",
            ContentType = "text/plain",
            SizeBytes = 5,
            Status = DocumentStatus.Failed,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
        };
        document.Ingestion.AttemptCount = 2;
        document.Ingestion.FailureReason = "previous";
        await repository.AddAsync(document, CancellationToken.None);

        var pipeline = new DocumentIngestionPipeline(
            repository,
            new SuccessProcessor(),
            TimeProvider.System);

        var result = await pipeline.RunAsync(id, CancellationToken.None);

        Assert.Equal(DocumentStatus.Ready, result.Status);
        Assert.Equal(3, result.Ingestion.AttemptCount);
        Assert.Null(result.Ingestion.FailureReason);
    }

    private sealed class SuccessProcessor : IDocumentIngestionProcessor
    {
        public Task ProcessAsync(Document document, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class FailingProcessor(string message) : IDocumentIngestionProcessor
    {
        public Task ProcessAsync(Document document, CancellationToken cancellationToken) =>
            throw new InvalidOperationException(message);
    }
}
