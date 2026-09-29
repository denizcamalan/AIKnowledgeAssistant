namespace AIKnowledgeAssistant.Domain.Documents;

public sealed class InvalidDocumentIngestionTransitionException : Exception
{
    public InvalidDocumentIngestionTransitionException(DocumentStatus current, DocumentStatus target)
        : base($"Cannot transition document ingestion from {current} to {target}.")
    {
        Current = current;
        Target = target;
    }

    public DocumentStatus Current { get; }

    public DocumentStatus Target { get; }
}
