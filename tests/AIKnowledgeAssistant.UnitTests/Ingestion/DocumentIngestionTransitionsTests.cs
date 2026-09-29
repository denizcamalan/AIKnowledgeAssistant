using AIKnowledgeAssistant.Domain.Documents;

namespace AIKnowledgeAssistant.UnitTests.Ingestion;

public sealed class DocumentIngestionTransitionsTests
{
    private static Document CreateDocument(DocumentStatus status) =>
        new()
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "a.txt",
            DisplayName = "a.txt",
            ContentType = "text/plain",
            SizeBytes = 1,
            Status = status,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
        };

    [Theory]
    [InlineData(DocumentStatus.Uploaded)]
    [InlineData(DocumentStatus.Failed)]
    public void BeginProcessing_FromEligibleStatus_SetsProcessingAndIncrementsAttempt(DocumentStatus initial)
    {
        var document = CreateDocument(initial);
        var now = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);

        DocumentIngestionTransitions.BeginProcessing(document, now);

        Assert.Equal(DocumentStatus.Processing, document.Status);
        Assert.Equal(1, document.Ingestion.AttemptCount);
        Assert.Equal(now, document.Ingestion.StartedAtUtc);
        Assert.Null(document.Ingestion.FailureReason);
        Assert.Null(document.Ingestion.CompletedAtUtc);
    }

    [Fact]
    public void BeginProcessing_FromReady_ThrowsInvalidTransition()
    {
        var document = CreateDocument(DocumentStatus.Ready);

        var exception = Assert.Throws<InvalidDocumentIngestionTransitionException>(() =>
            DocumentIngestionTransitions.BeginProcessing(document, DateTimeOffset.UtcNow));

        Assert.Equal(DocumentStatus.Ready, exception.Current);
        Assert.Equal(DocumentStatus.Processing, exception.Target);
    }

    [Fact]
    public void MarkReady_FromProcessing_SetsReadyAndCompletedTimestamp()
    {
        var document = CreateDocument(DocumentStatus.Processing);
        var now = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);

        DocumentIngestionTransitions.MarkReady(document, now);

        Assert.Equal(DocumentStatus.Ready, document.Status);
        Assert.Equal(now, document.Ingestion.CompletedAtUtc);
    }

    [Fact]
    public void MarkFailed_FromProcessing_PersistsFailureReason()
    {
        var document = CreateDocument(DocumentStatus.Processing);
        var now = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);

        DocumentIngestionTransitions.MarkFailed(document, "extract failed", now);

        Assert.Equal(DocumentStatus.Failed, document.Status);
        Assert.Equal("extract failed", document.Ingestion.FailureReason);
        Assert.Equal(now, document.Ingestion.CompletedAtUtc);
    }

    [Fact]
    public void MarkReady_FromUploaded_ThrowsInvalidTransition()
    {
        var document = CreateDocument(DocumentStatus.Uploaded);

        Assert.Throws<InvalidDocumentIngestionTransitionException>(() =>
            DocumentIngestionTransitions.MarkReady(document, DateTimeOffset.UtcNow));
    }
}
