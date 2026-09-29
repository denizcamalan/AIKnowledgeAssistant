namespace AIKnowledgeAssistant.Application.Chat.Tokens;

public sealed record TokenUsage(
    int? PromptTokens,
    int? CompletionTokens,
    int EstimatedPromptTokens,
    int EstimatedCompletionTokens)
{
    public int TotalReportedTokens =>
        (PromptTokens ?? EstimatedPromptTokens) + (CompletionTokens ?? EstimatedCompletionTokens);
}
