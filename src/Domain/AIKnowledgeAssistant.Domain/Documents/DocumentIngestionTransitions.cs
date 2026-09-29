namespace AIKnowledgeAssistant.Domain.Documents;

public static class DocumentIngestionTransitions
{
    public static void BeginProcessing(Document document, DateTimeOffset utcNow)
    {
        if (document.Status is not DocumentStatus.Uploaded and not DocumentStatus.Failed)
        {
            throw new InvalidDocumentIngestionTransitionException(document.Status, DocumentStatus.Processing);
        }

        document.Status = DocumentStatus.Processing;
        document.Ingestion.AttemptCount++;
        document.Ingestion.StartedAtUtc = utcNow;
        document.Ingestion.CompletedAtUtc = null;
        document.Ingestion.FailureReason = null;
        document.UpdatedAtUtc = utcNow;
    }

    public static void MarkReady(Document document, DateTimeOffset utcNow)
    {
        if (document.Status != DocumentStatus.Processing)
        {
            throw new InvalidDocumentIngestionTransitionException(document.Status, DocumentStatus.Ready);
        }

        document.Status = DocumentStatus.Ready;
        document.Ingestion.CompletedAtUtc = utcNow;
        document.Ingestion.FailureReason = null;
        document.UpdatedAtUtc = utcNow;
    }

    public static void MarkFailed(Document document, string failureReason, DateTimeOffset utcNow)
    {
        if (document.Status != DocumentStatus.Processing)
        {
            throw new InvalidDocumentIngestionTransitionException(document.Status, DocumentStatus.Failed);
        }

        if (string.IsNullOrWhiteSpace(failureReason))
        {
            throw new ArgumentException("Failure reason is required.", nameof(failureReason));
        }

        document.Status = DocumentStatus.Failed;
        document.Ingestion.FailureReason = failureReason.Trim();
        document.Ingestion.CompletedAtUtc = utcNow;
        document.UpdatedAtUtc = utcNow;
    }
}
