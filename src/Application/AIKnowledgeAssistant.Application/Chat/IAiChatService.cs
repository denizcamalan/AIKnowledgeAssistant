namespace AIKnowledgeAssistant.Application.Chat;

public interface IAiChatService
{
    Task<ChatReply> CompleteAsync(ChatPrompt prompt, CancellationToken cancellationToken);
}

public sealed record ChatPrompt(
    string Message,
    string? SystemMessage = null,
    IReadOnlyList<ChatMessage>? FewShotExamples = null);

public sealed record ChatReply(
    string Content,
    string Model,
    string Provider,
    Tokens.TokenUsage TokenUsage,
    long? ProviderDurationMs = null);
