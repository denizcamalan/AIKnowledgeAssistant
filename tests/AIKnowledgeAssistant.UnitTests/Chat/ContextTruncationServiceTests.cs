using AIKnowledgeAssistant.Application.Chat.Tokens;

namespace AIKnowledgeAssistant.UnitTests.Chat;

public sealed class ContextTruncationServiceTests
{
    [Fact]
    public void TruncateToTokenBudget_WhenWithinBudget_DoesNotTruncate()
    {
        var service = new HeadTailContextTruncationService(new HeuristicTokenEstimator());

        var result = service.TruncateToTokenBudget("kısa metin", maxTokenBudget: 100);

        Assert.False(result.WasTruncated);
        Assert.Equal("kısa metin", result.Text);
    }

    [Fact]
    public void TruncateToTokenBudget_WhenTooLong_AppliesHeadTailStrategy()
    {
        var service = new HeadTailContextTruncationService(new HeuristicTokenEstimator());
        var longText = string.Join(' ', Enumerable.Repeat("uzun", 5000));

        var result = service.TruncateToTokenBudget(longText, maxTokenBudget: 200);

        Assert.True(result.WasTruncated);
        Assert.Contains("truncated for token budget", result.Text, StringComparison.Ordinal);
        Assert.True(result.ResultTokenEstimate <= 200);
    }
}
