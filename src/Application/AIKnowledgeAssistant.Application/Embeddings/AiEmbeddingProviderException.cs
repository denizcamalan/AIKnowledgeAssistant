namespace AIKnowledgeAssistant.Application.Embeddings;

public sealed class AiEmbeddingProviderException : Exception
{
    public AiEmbeddingProviderException(string message)
        : base(message)
    {
    }

    public AiEmbeddingProviderException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
