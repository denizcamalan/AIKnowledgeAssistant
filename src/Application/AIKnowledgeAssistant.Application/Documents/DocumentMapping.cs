using AIKnowledgeAssistant.Domain.Documents;

namespace AIKnowledgeAssistant.Application.Documents;

internal static class DocumentMapping
{
    public static DocumentSummaryResult ToSummary(Document document) =>
        new(
            document.Id,
            document.DisplayName,
            document.OriginalFileName,
            document.Status,
            document.SizeBytes,
            document.CreatedAtUtc,
            ToIngestionMetadata(document));

    public static DocumentDetailResult ToDetail(Document document) =>
        new(
            document.Id,
            document.DisplayName,
            document.OriginalFileName,
            document.ContentType,
            document.Status,
            document.SizeBytes,
            document.CreatedAtUtc,
            document.UpdatedAtUtc,
            ToIngestionMetadata(document));

    private static DocumentIngestionMetadataResult ToIngestionMetadata(Document document) =>
        new(
            document.Ingestion.AttemptCount,
            document.Ingestion.FailureReason,
            document.Ingestion.StartedAtUtc,
            document.Ingestion.CompletedAtUtc);
}
