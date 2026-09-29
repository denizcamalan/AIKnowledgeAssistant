using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AIKnowledgeAssistant.Application.Chat;
using AIKnowledgeAssistant.Application.Configuration;
using AIKnowledgeAssistant.Infrastructure.Chat;
using Microsoft.Extensions.Options;

namespace AIKnowledgeAssistant.UnitTests.Chat;

public sealed class OllamaChatCompletionClientTests
{
    [Fact]
    public async Task CompleteAsync_WithSuccessfulResponse_ReturnsAssistantContent()
    {
        var handler = new StubHttpMessageHandler(_ =>
        {
            var json = JsonSerializer.Serialize(new
            {
                message = new { role = "assistant", content = "Merhaba!" },
            });
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
            };
        });

        var client = CreateClient(handler);

        var result = await client.CompleteAsync(
            new ChatCompletionRequest(
                [new ChatMessage("user", "Selam")],
                Model: string.Empty),
            CancellationToken.None);

        Assert.Equal("Merhaba!", result.Content);
        Assert.Equal("qwen3:4b", result.Model);
        Assert.Equal(LlmProviders.Ollama, result.Provider);
    }

    [Fact]
    public async Task CompleteAsync_WhenModelMissing_ReturnsHelpfulError()
    {
        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.NotFound));

        var client = CreateClient(handler);

        var exception = await Assert.ThrowsAsync<AiChatProviderException>(() =>
            client.CompleteAsync(
                new ChatCompletionRequest([new ChatMessage("user", "Hi")], Model: string.Empty),
                CancellationToken.None));

        Assert.Contains("ollama pull", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static OllamaChatCompletionClient CreateClient(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:11434/") };
        var options = Options.Create(new LlmOptions { Ollama = new OllamaOptions { Model = "qwen3:4b" } });
        return new OllamaChatCompletionClient(httpClient, options);
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) =>
            _responder = responder;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal(new Uri("http://localhost:11434/api/chat"), request.RequestUri);
            return Task.FromResult(_responder(request));
        }
    }
}
