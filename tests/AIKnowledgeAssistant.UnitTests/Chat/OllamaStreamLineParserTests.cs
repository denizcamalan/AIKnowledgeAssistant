using AIKnowledgeAssistant.Application.Chat;
using AIKnowledgeAssistant.Infrastructure.Chat;

namespace AIKnowledgeAssistant.UnitTests.Chat;

public sealed class OllamaStreamLineParserTests
{
    [Fact]
    public void ParseLine_WithDeltaAndDone_ProducesChunks()
    {
        const string deltaLine = """{"message":{"role":"assistant","content":"Hi"},"done":false}""";
        const string doneLine =
            """{"message":{"role":"assistant","content":""},"done":true,"prompt_eval_count":10,"eval_count":2,"total_duration":5000000}""";

        var deltas = OllamaStreamLineParser.ParseLine(deltaLine, "qwen3:4b").ToList();
        Assert.Single(deltas);
        Assert.Equal("Hi", deltas[0].TextDelta);

        var done = OllamaStreamLineParser.ParseLine(doneLine, "qwen3:4b").ToList();
        Assert.Single(done);
        Assert.True(done[0].IsFinal);
        Assert.Equal(10, done[0].TokenUsage?.PromptTokens);
    }
}
