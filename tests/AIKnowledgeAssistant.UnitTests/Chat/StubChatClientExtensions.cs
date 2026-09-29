using System.Runtime.CompilerServices;
using AIKnowledgeAssistant.Application.Chat;
using AIKnowledgeAssistant.Application.Chat.Tokens;

namespace AIKnowledgeAssistant.UnitTests.Chat;

internal static class StubChatClientExtensions
{
    public static async IAsyncEnumerable<ChatStreamChunk> StreamFromCompleteAsync(
        this IAiChatCompletionClient client,
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var result = await client.CompleteAsync(request, cancellationToken);
        yield return new ChatStreamChunk(result.Content, IsFinal: false);
        yield return new ChatStreamChunk(
            string.Empty,
            IsFinal: true,
            result.Model,
            result.Provider,
            result.TokenUsage,
            result.ProviderDurationMs);
    }
}
