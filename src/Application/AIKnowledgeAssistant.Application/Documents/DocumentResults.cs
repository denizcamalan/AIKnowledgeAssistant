using AIKnowledgeAssistant.Domain.Documents;

namespace AIKnowledgeAssistant.Application.Documents;

public sealed record DocumentIngestionMetadataResult(
    int AttemptCount,
    string? FailureReason,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? CompletedAtUtc);

public sealed record DocumentSummaryResult(
    Guid Id,
    string DisplayName,
    string OriginalFileName,
    DocumentStatus Status,
    long SizeBytes,
    DateTimeOffset CreatedAtUtc,
    DocumentIngestionMetadataResult Ingestion);

public sealed record DocumentDetailResult(
    Guid Id,
    string DisplayName,
    string OriginalFileName,
    string ContentType,
    DocumentStatus Status,
    long SizeBytes,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DocumentIngestionMetadataResult Ingestion);
