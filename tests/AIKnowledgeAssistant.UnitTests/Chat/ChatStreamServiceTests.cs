using AIKnowledgeAssistant.Application.Chat;
using AIKnowledgeAssistant.Application.Chat.Tokens;
using Microsoft.Extensions.Logging.Abstractions;

namespace AIKnowledgeAssistant.UnitTests.Chat;

public sealed class ChatStreamServiceTests
{
    [Fact]
    public async Task StreamAsync_ForwardsChunksFromClient()
    {
        var service = new ChatStreamService(
            new StubChatClientFactory(new StreamingStubClient()),
            new HeuristicTokenEstimator(),
            NullLogger<ChatStreamService>.Instance);

        var chunks = new List<ChatStreamChunk>();
        await foreach (var chunk in service.StreamAsync(new ChatPrompt("Hi"), CancellationToken.None))
        {
            chunks.Add(chunk);
        }

        Assert.Equal(3, chunks.Count);
        Assert.Equal("Hello", chunks[0].TextDelta);
        Assert.True(chunks[^1].IsFinal);
    }

    [Fact]
    public async Task StreamAsync_WhenCancelled_PropagatesCancellation()
    {
        var service = new ChatStreamService(
            new StubChatClientFactory(new CancellingStubClient()),
            new HeuristicTokenEstimator(),
            NullLogger<ChatStreamService>.Instance);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(30);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var _ in service.StreamAsync(new ChatPrompt("Slow"), cts.Token))
            {
            }
        });
    }

    private sealed class CancellingStubClient : IAiChatCompletionClient
    {
        public Task<ChatCompletionResult> CompleteAsync(
            ChatCompletionRequest request,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public async IAsyncEnumerable<ChatStreamChunk> StreamAsync(
            ChatCompletionRequest request,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            yield return new ChatStreamChunk("x", IsFinal: false);
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        }
    }

    private sealed class StubChatClientFactory(IAiChatCompletionClient client) : IAiChatClientFactory
    {
        public IAiChatCompletionClient GetClient() => client;
    }

    private sealed class StreamingStubClient : IAiChatCompletionClient
    {
        public Task<ChatCompletionResult> CompleteAsync(
            ChatCompletionRequest request,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public async IAsyncEnumerable<ChatStreamChunk> StreamAsync(
            ChatCompletionRequest request,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            yield return new ChatStreamChunk("Hello", IsFinal: false);
            yield return new ChatStreamChunk("!", IsFinal: false);
            yield return new ChatStreamChunk(
                string.Empty,
                IsFinal: true,
                "model",
                "Stub",
                new TokenUsage(1, 1, 1, 1));
            await Task.CompletedTask;
        }
    }
}
