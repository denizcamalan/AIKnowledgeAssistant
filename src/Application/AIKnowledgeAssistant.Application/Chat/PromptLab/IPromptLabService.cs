namespace AIKnowledgeAssistant.Application.Chat.PromptLab;

public interface IPromptLabService
{
    Task<PromptLabComparisonResult> CompareVariantsAsync(
        PromptLabInput input,
        CancellationToken cancellationToken);
}

public sealed record PromptLabInput(string Question, string? GroundingContext = null);

public sealed record PromptLabComparisonResult(
    string Question,
    string GroundingContextUsed,
    IReadOnlyList<PromptVariantRunResult> Variants,
    IReadOnlyList<string> EvaluationCriteria);

public sealed record PromptVariantRunResult(
    string Variant,
    string Intent,
    string SystemMessage,
    string UserMessage,
    int FewShotTurnCount,
    string AssistantMessage,
    string Model,
    string Provider,
    long DurationMs,
    int ResponseCharacterCount);
