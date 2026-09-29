namespace AIKnowledgeAssistant.Application.Chat;

public interface IAiChatCompletionClient
{
    Task<ChatCompletionResult> CompleteAsync(ChatCompletionRequest request, CancellationToken cancellationToken);

    IAsyncEnumerable<ChatStreamChunk> StreamAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken);
}

public sealed record ChatCompletionRequest(
    IReadOnlyList<ChatMessage> Messages,
    string Model,
    bool RequestJsonFormat = false);

public sealed record ChatMessage(string Role, string Content);

public sealed record ChatCompletionResult(
    string Content,
    string Model,
    string Provider,
    Tokens.TokenUsage TokenUsage,
    long? ProviderDurationMs = null);
