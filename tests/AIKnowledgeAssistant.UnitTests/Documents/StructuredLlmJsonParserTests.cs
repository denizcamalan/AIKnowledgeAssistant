using AIKnowledgeAssistant.Application.Documents.StructuredOutput;

namespace AIKnowledgeAssistant.UnitTests.Documents;

public sealed class StructuredLlmJsonParserTests
{
    private readonly StructuredLlmJsonParser _parser = new();

    [Fact]
    public void ParseClassification_WithValidJson_ReturnsPayload()
    {
        const string json =
            """
            {"summary":"Kısa özet","category":"policy","keywords":["remote","work"],"confidence":0.82}
            """;

        var result = _parser.ParseClassification(json);

        Assert.Equal("policy", result.Value.Category);
        Assert.Equal(0.82, result.Value.Confidence);
    }

    [Fact]
    public void ParseClassification_WithMarkdownFence_ExtractsJson()
    {
        const string raw =
            """
            ```json
            {"summary":"Özet","category":"technical","keywords":["api"],"confidence":0.5}
            ```
            """;

        var result = _parser.ParseClassification(raw);

        Assert.Equal("technical", result.Value.Category);
    }

    [Fact]
    public void ParseClassification_WithInvalidCategory_Throws()
    {
        const string json =
            """
            {"summary":"Özet","category":"finance","keywords":["x"],"confidence":0.5}
            """;

        Assert.Throws<StructuredOutputParseException>(() => _parser.ParseClassification(json));
    }
}
