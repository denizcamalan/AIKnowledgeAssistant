using AIKnowledgeAssistant.Domain.Documents;

namespace AIKnowledgeAssistant.Application.Documents;

public sealed record DocumentSummaryResult(
    Guid Id,
    string DisplayName,
    string OriginalFileName,
    DocumentStatus Status,
    long SizeBytes,
    DateTimeOffset CreatedAtUtc);

public sealed record DocumentDetailResult(
    Guid Id,
    string DisplayName,
    string OriginalFileName,
    string ContentType,
    DocumentStatus Status,
    long SizeBytes,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
