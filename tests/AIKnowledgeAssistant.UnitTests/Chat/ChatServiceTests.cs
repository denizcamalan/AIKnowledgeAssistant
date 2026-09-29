using AIKnowledgeAssistant.Application.Chat;
using AIKnowledgeAssistant.Application.Chat.Tokens;
using Microsoft.Extensions.Logging.Abstractions;

namespace AIKnowledgeAssistant.UnitTests.Chat;

public sealed class ChatServiceTests
{
    [Fact]
    public async Task CompleteAsync_UsesFactoryClientAndMapsReply()
    {
        var factory = new StubChatClientFactory(new StubChatCompletionClient("answer", "test-model", "Stub"));
        var service = CreateService(factory);

        var reply = await service.CompleteAsync(new ChatPrompt("Question?"), CancellationToken.None);

        Assert.Equal("answer", reply.Content);
        Assert.Equal("test-model", reply.Model);
        Assert.Equal("Stub", reply.Provider);
        Assert.Equal(12, reply.TokenUsage.PromptTokens);
    }

    [Fact]
    public async Task CompleteAsync_WithBlankMessage_Throws()
    {
        var service = CreateService(new StubChatClientFactory(new StubChatCompletionClient("x", "m", "p")));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CompleteAsync(new ChatPrompt("   "), CancellationToken.None));
    }

    private static ChatService CreateService(StubChatClientFactory factory) =>
        new(factory, new HeuristicTokenEstimator(), NullLogger<ChatService>.Instance);

    private sealed class StubChatClientFactory(IAiChatCompletionClient client) : IAiChatClientFactory
    {
        public IAiChatCompletionClient GetClient() => client;
    }

    private sealed class StubChatCompletionClient(string content, string model, string provider)
        : IAiChatCompletionClient
    {
        public Task<ChatCompletionResult> CompleteAsync(
            ChatCompletionRequest request,
            CancellationToken cancellationToken)
        {
            Assert.Single(request.Messages);
            Assert.Equal("user", request.Messages[0].Role);
            Assert.Equal("Question?", request.Messages[0].Content);
            return Task.FromResult(new ChatCompletionResult(
                content,
                model,
                provider,
                new TokenUsage(12, 3, 5, 3)));
        }
    }
}
