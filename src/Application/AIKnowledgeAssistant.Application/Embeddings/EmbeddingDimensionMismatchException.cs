namespace AIKnowledgeAssistant.Application.Embeddings;

public sealed class EmbeddingDimensionMismatchException : Exception
{
    public EmbeddingDimensionMismatchException(int expected, int actual)
        : base($"Embedding dimension mismatch. Expected {expected}, got {actual}.")
    {
        Expected = expected;
        Actual = actual;
    }

    public int Expected { get; }

    public int Actual { get; }
}
