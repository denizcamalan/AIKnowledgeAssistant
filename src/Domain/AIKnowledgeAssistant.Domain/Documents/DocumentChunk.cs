namespace AIKnowledgeAssistant.Domain.Documents;

public sealed class DocumentChunk
{
    public Guid Id { get; set; }

    public Guid DocumentId { get; set; }

    public int ChunkIndex { get; set; }

    public string Text { get; set; } = string.Empty;

    public int StartOffset { get; set; }

    public int EndOffset { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public Document? Document { get; set; }
}
