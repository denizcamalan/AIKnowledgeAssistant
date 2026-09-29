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

    /// <summary>Model context window (learning default for small local models).</summary>
    public int ContextWindowTokens { get; set; } = 8192;

    /// <summary>Max input budget before truncation strategies apply in the token lab.</summary>
    public int MaxPromptTokens { get; set; } = 2048;

    /// <summary>Reserved completion headroom subtracted from MaxPromptTokens for context budget.</summary>
    public int CompletionTokenReserve { get; set; } = 512;

    /// <summary>USD per 1K input tokens for cost illustration (0 for local Ollama).</summary>
    public decimal EstimatedCostPer1KInputTokens { get; set; }

    public int StructuredOutputMaxAttempts { get; set; } = 3;
}

public sealed class OllamaOptions
{
    public string BaseUrl { get; set; } = "http://localhost:11434";

    public string Model { get; set; } = "qwen3:4b";
}
