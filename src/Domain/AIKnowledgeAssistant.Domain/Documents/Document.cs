namespace AIKnowledgeAssistant.Domain.Documents;

public sealed class Document
{
    public Guid Id { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public string StoragePath { get; set; } = string.Empty;

    public DocumentStatus Status { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}
