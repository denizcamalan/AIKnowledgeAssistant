using AIKnowledgeAssistant.Application.Chat;

namespace AIKnowledgeAssistant.UnitTests.Chat;

public sealed class ChatServiceTests
{
    [Fact]
    public async Task CompleteAsync_UsesFactoryClientAndMapsReply()
    {
        var factory = new StubChatClientFactory(new StubChatCompletionClient("answer", "test-model", "Stub"));
        var service = new ChatService(factory);

        var reply = await service.CompleteAsync(new ChatPrompt("Question?"), CancellationToken.None);

        Assert.Equal("answer", reply.Content);
        Assert.Equal("test-model", reply.Model);
        Assert.Equal("Stub", reply.Provider);
    }

    [Fact]
    public async Task CompleteAsync_WithBlankMessage_Throws()
    {
        var service = new ChatService(new StubChatClientFactory(new StubChatCompletionClient("x", "m", "p")));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CompleteAsync(new ChatPrompt("   "), CancellationToken.None));
    }

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
            return Task.FromResult(new ChatCompletionResult(content, model, provider));
        }
    }
}
