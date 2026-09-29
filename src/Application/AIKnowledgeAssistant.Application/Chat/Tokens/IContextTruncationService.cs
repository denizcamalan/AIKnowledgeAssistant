namespace AIKnowledgeAssistant.Application.Chat.Tokens;

public interface IContextTruncationService
{
    ContextTruncationResult TruncateToTokenBudget(string text, int maxTokenBudget);
}

public sealed record ContextTruncationResult(
    string Text,
    bool WasTruncated,
    int OriginalTokenEstimate,
    int ResultTokenEstimate,
    string Strategy);

public sealed class HeadTailContextTruncationService : IContextTruncationService
{
    public const string StrategyName = "head-tail-60-35";
    private const string Marker = "\n…[truncated for token budget]…\n";

    private readonly ITokenEstimator _estimator;

    public HeadTailContextTruncationService(ITokenEstimator estimator) => _estimator = estimator;

    public ContextTruncationResult TruncateToTokenBudget(string text, int maxTokenBudget)
    {
        if (maxTokenBudget <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxTokenBudget));
        }

        if (string.IsNullOrEmpty(text))
        {
            return new ContextTruncationResult(text, false, 0, 0, StrategyName);
        }

        var originalEstimate = _estimator.Estimate(text);
        if (originalEstimate <= maxTokenBudget)
        {
            return new ContextTruncationResult(text, false, originalEstimate, originalEstimate, StrategyName);
        }

        var markerTokens = _estimator.Estimate(Marker);
        var bodyBudget = Math.Max(1, maxTokenBudget - markerTokens);
        var headChars = (int)(text.Length * 0.60);
        var tailChars = (int)(text.Length * 0.35);
        var truncated = text[..headChars] + Marker + text[^tailChars..];

        while (_estimator.Estimate(truncated) > maxTokenBudget && headChars + tailChars > 2)
        {
            headChars = (int)(headChars * 0.9);
            tailChars = (int)(tailChars * 0.9);
            truncated = text[..Math.Max(1, headChars)] + Marker + text[^Math.Max(1, tailChars)..];
        }

        var resultEstimate = _estimator.Estimate(truncated);
        return new ContextTruncationResult(truncated, true, originalEstimate, resultEstimate, StrategyName);
    }
}
