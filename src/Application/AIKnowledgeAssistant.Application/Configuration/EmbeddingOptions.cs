namespace AIKnowledgeAssistant.Application.Configuration;

public sealed class EmbeddingOptions
{
    public const string SectionName = "Embeddings";

    public string Provider { get; set; } = LlmProviders.Ollama;

    public string Model { get; set; } = "nomic-embed-text";

    public int ExpectedDimensions { get; set; } = 768;

    public int BatchSize { get; set; } = 16;

    public int MaxAttempts { get; set; } = 3;
}
