namespace AIKnowledgeAssistant.Application.Chat.Tokens;

/// <summary>
/// Rough token estimate for budgeting before a provider call (learning heuristic; not a tokenizer).
/// </summary>
public interface ITokenEstimator
{
    int Estimate(string? text);
}

public sealed class HeuristicTokenEstimator : ITokenEstimator
{
    /// <summary>Average ~4 characters per token for Latin scripts (approximation).</summary>
    public int Estimate(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        return (int)Math.Ceiling(text.Length / 4.0);
    }
}
