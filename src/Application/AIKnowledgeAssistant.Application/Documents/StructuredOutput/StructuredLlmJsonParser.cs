using System.Text.Json;
using System.Text.RegularExpressions;

namespace AIKnowledgeAssistant.Application.Documents.StructuredOutput;

public interface IStructuredLlmJsonParser
{
    StructuredParseResult<DocumentClassificationPayload> ParseClassification(string rawModelOutput);
}

public sealed partial class StructuredLlmJsonParser : IStructuredLlmJsonParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public StructuredParseResult<DocumentClassificationPayload> ParseClassification(string rawModelOutput)
    {
        if (string.IsNullOrWhiteSpace(rawModelOutput))
        {
            throw new StructuredOutputParseException("Model output was empty.");
        }

        var json = ExtractJsonPayload(rawModelOutput);
        DocumentClassificationPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<DocumentClassificationPayload>(json, JsonOptions);
        }
        catch (JsonException exception)
        {
            throw new StructuredOutputParseException($"Invalid JSON: {exception.Message}");
        }

        if (payload is null)
        {
            throw new StructuredOutputParseException("JSON deserialized to null.");
        }

        Validate(payload);
        return new StructuredParseResult<DocumentClassificationPayload>(payload, json);
    }

    internal static void Validate(DocumentClassificationPayload payload)
    {
        if (string.IsNullOrWhiteSpace(payload.Summary) || payload.Summary.Length > DocumentClassificationSchema.MaxSummaryLength)
        {
            throw new StructuredOutputParseException("Summary must be between 1 and 500 characters.");
        }

        if (!DocumentClassificationSchema.AllowedCategories.Contains(payload.Category))
        {
            throw new StructuredOutputParseException(
                $"Category must be one of: {string.Join(", ", DocumentClassificationSchema.AllowedCategories)}.");
        }

        if (payload.Keywords.Count is 0 or > DocumentClassificationSchema.MaxKeywords)
        {
            throw new StructuredOutputParseException("Keywords must contain 1 to 8 items.");
        }

        if (payload.Keywords.Any(keyword => string.IsNullOrWhiteSpace(keyword)))
        {
            throw new StructuredOutputParseException("Keywords cannot contain blank entries.");
        }

        if (payload.Confidence is < 0 or > 1)
        {
            throw new StructuredOutputParseException("Confidence must be between 0 and 1.");
        }
    }

    internal static string ExtractJsonPayload(string raw)
    {
        var trimmed = raw.Trim();
        var fenced = JsonFenceRegex().Match(trimmed);
        if (fenced.Success)
        {
            return fenced.Groups["json"].Value.Trim();
        }

        var start = trimmed.IndexOf('{');
        var end = trimmed.LastIndexOf('}');
        if (start >= 0 && end > start)
        {
            return trimmed[start..(end + 1)];
        }

        return trimmed;
    }

    [GeneratedRegex(@"```(?:json)?\s*(?<json>\{[\s\S]*?\})\s*```", RegexOptions.IgnoreCase)]
    private static partial Regex JsonFenceRegex();
}
