namespace AIKnowledgeAssistant.Api.Configuration;

public sealed class CorsSettings
{
    public const string SectionName = "Cors";

    public string[] Origins { get; init; } = ["http://localhost:5173"];
}
