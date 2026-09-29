namespace AIKnowledgeAssistant.Application.Chat.Tokens;

public interface ITokenContextLabService
{
    Task<TokenContextExperimentResult> RunContextExperimentAsync(
        TokenContextExperimentInput input,
        CancellationToken cancellationToken);
}

public sealed record TokenContextExperimentInput(string Question, string? GroundingContext = null);

public sealed record TokenContextExperimentResult(
    string Question,
    int ContextWindowTokens,
    int MaxPromptTokens,
    IReadOnlyList<TokenContextScenarioResult> Scenarios,
    IReadOnlyList<string> Notes);

public sealed record TokenContextScenarioResult(
    string Scenario,
    string Description,
    bool TruncationApplied,
    string TruncationStrategy,
    int ContextCharacterCount,
    int EstimatedInputTokensBeforeCall,
    TokenUsage TokenUsage,
    long DurationMs,
    decimal EstimatedInputCost,
    string AssistantPreview);
