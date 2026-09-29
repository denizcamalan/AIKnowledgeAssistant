namespace AIKnowledgeAssistant.Domain.Documents;

/// <summary>
/// Retry and timing metadata for the ingestion pipeline (separate from <see cref="DocumentStatus"/>).
/// </summary>
public sealed class DocumentIngestionState
{
    public int AttemptCount { get; set; }

    public string? FailureReason { get; set; }

    public DateTimeOffset? StartedAtUtc { get; set; }

    public DateTimeOffset? CompletedAtUtc { get; set; }
}
