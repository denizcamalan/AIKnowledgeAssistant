using AIKnowledgeAssistant.Application.Chat.Tokens;
using Microsoft.Extensions.Logging;

namespace AIKnowledgeAssistant.Application.Chat;

public sealed record ChatStreamChunk(
    string TextDelta,
    bool IsFinal,
    string? Model = null,
    string? Provider = null,
    TokenUsage? TokenUsage = null,
    long? ProviderDurationMs = null);

public interface IChatStreamService
{
    IAsyncEnumerable<ChatStreamChunk> StreamAsync(ChatPrompt prompt, CancellationToken cancellationToken);
}

public sealed class ChatStreamService : IChatStreamService
{
    private readonly IAiChatClientFactory _clientFactory;
    private readonly ITokenEstimator _tokenEstimator;
    private readonly ILogger<ChatStreamService> _logger;

    public ChatStreamService(
        IAiChatClientFactory clientFactory,
        ITokenEstimator tokenEstimator,
        ILogger<ChatStreamService> logger)
    {
        _clientFactory = clientFactory;
        _tokenEstimator = tokenEstimator;
        _logger = logger;
    }

    public async IAsyncEnumerable<ChatStreamChunk> StreamAsync(
        ChatPrompt prompt,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(prompt.Message))
        {
            throw new ArgumentException("Message is required.", nameof(prompt));
        }

        if (prompt.RequestJsonFormat)
        {
            throw new ArgumentException("JSON response format is not supported for streaming chat.", nameof(prompt));
        }

        var client = _clientFactory.GetClient();
        var messages = ChatMessageBuilder.BuildMessages(prompt);
        var estimatedPrompt = _tokenEstimator.Estimate(ChatMessageBuilder.SerializeForEstimate(messages));

        await foreach (var chunk in client.StreamAsync(
                           new ChatCompletionRequest(messages, Model: string.Empty, RequestJsonFormat: false),
                           cancellationToken))
        {
            if (chunk.IsFinal)
            {
                _logger.LogInformation(
                    "Chat stream completed Provider={Provider} Model={Model} PromptTokens={PromptTokens} CompletionTokens={CompletionTokens} EstimatedPrompt={EstimatedPrompt}",
                    chunk.Provider,
                    chunk.Model,
                    chunk.TokenUsage?.PromptTokens,
                    chunk.TokenUsage?.CompletionTokens,
                    estimatedPrompt);
            }

            yield return chunk;
        }
    }
}
