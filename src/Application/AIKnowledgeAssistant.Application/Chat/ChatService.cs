using AIKnowledgeAssistant.Application.Chat.Tokens;
using Microsoft.Extensions.Logging;

namespace AIKnowledgeAssistant.Application.Chat;

public sealed class ChatService : IAiChatService
{
    private readonly IAiChatClientFactory _clientFactory;
    private readonly ITokenEstimator _tokenEstimator;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        IAiChatClientFactory clientFactory,
        ITokenEstimator tokenEstimator,
        ILogger<ChatService> logger)
    {
        _clientFactory = clientFactory;
        _tokenEstimator = tokenEstimator;
        _logger = logger;
    }

    public async Task<ChatReply> CompleteAsync(ChatPrompt prompt, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(prompt.Message))
        {
            throw new ArgumentException("Message is required.", nameof(prompt));
        }

        var client = _clientFactory.GetClient();
        var messages = ChatMessageBuilder.BuildMessages(prompt);
        var estimatedPromptTokens = _tokenEstimator.Estimate(ChatMessageBuilder.SerializeForEstimate(messages));

        var completion = await client.CompleteAsync(
            new ChatCompletionRequest(messages, Model: string.Empty, prompt.RequestJsonFormat),
            cancellationToken);

        _logger.LogInformation(
            "Chat completion Provider={Provider} Model={Model} PromptTokens={PromptTokens} CompletionTokens={CompletionTokens} EstimatedPrompt={EstimatedPrompt} ProviderDurationMs={ProviderDurationMs}",
            completion.Provider,
            completion.Model,
            completion.TokenUsage.PromptTokens,
            completion.TokenUsage.CompletionTokens,
            estimatedPromptTokens,
            completion.ProviderDurationMs);

        return new ChatReply(
            completion.Content,
            completion.Model,
            completion.Provider,
            completion.TokenUsage,
            completion.ProviderDurationMs);
    }
}
