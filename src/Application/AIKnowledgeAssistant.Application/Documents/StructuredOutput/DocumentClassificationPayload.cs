using System.Text.Json.Serialization;

namespace AIKnowledgeAssistant.Application.Documents.StructuredOutput;

public sealed class DocumentClassificationPayload
{
    [JsonPropertyName("summary")]
    public string Summary { get; init; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; init; } = string.Empty;

    [JsonPropertyName("keywords")]
    public IReadOnlyList<string> Keywords { get; init; } = Array.Empty<string>();

    [JsonPropertyName("confidence")]
    public double Confidence { get; init; }
}

public static class DocumentClassificationSchema
{
    public static readonly IReadOnlySet<string> AllowedCategories =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "policy",
            "technical",
            "general",
        };

    public const int MaxSummaryLength = 500;
    public const int MaxKeywords = 8;
}

public sealed record StructuredParseResult<T>(T Value, string NormalizedJson);

public sealed class StructuredOutputParseException : Exception
{
    public StructuredOutputParseException(string message)
        : base(message)
    {
    }
}
