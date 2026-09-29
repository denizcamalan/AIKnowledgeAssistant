namespace AIKnowledgeAssistant.Application.Configuration;

public static class LlmProviders
{
    public const string Ollama = "Ollama";
}

public sealed class LlmOptions
{
    public const string SectionName = "Llm";

    public string Provider { get; set; } = LlmProviders.Ollama;

    public OllamaOptions Ollama { get; set; } = new();
}

public sealed class OllamaOptions
{
    public string BaseUrl { get; set; } = "http://localhost:11434";

    public string Model { get; set; } = "qwen3:4b";
}
