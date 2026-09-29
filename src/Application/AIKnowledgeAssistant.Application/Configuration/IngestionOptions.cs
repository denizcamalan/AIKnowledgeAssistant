namespace AIKnowledgeAssistant.Application.Configuration;

public sealed class IngestionOptions
{
    public const string SectionName = "Ingestion";

    public int ChunkSize { get; set; } = 800;

    public int ChunkOverlap { get; set; } = 120;

    public string[] TextExtensions { get; set; } = [".txt", ".md"];
}
