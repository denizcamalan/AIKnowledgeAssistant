namespace AIKnowledgeAssistant.Domain.Documents;

public sealed class DocumentNotFoundException : Exception
{
    public DocumentNotFoundException(Guid id)
        : base($"Document '{id}' was not found.")
    {
        DocumentId = id;
    }

    public Guid DocumentId { get; }
}
