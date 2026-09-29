namespace AIKnowledgeAssistant.Application.Chat;

public interface IAiChatCompletionClient
{
    Task<ChatCompletionResult> CompleteAsync(ChatCompletionRequest request, CancellationToken cancellationToken);
}

public sealed record ChatCompletionRequest(
    IReadOnlyList<ChatMessage> Messages,
    string Model);

public sealed record ChatMessage(string Role, string Content);

public sealed record ChatCompletionResult(
    string Content,
    string Model,
    string Provider,
    Tokens.TokenUsage TokenUsage,
    long? ProviderDurationMs = null);
