using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using AIKnowledgeAssistant.Api.Contracts.Auth;
using AIKnowledgeAssistant.Api.Contracts.Chat;
using AIKnowledgeAssistant.Application.Chat;
using AIKnowledgeAssistant.Application.Chat.Tokens;
using AIKnowledgeAssistant.Application.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AIKnowledgeAssistant.IntegrationTests;

public sealed class ChatStreamEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ChatStreamEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IAiChatClientFactory>();
                services.AddSingleton<IAiChatClientFactory, StreamingStubAiChatClientFactory>();
            });
        }).CreateClient();
    }

    [Fact]
    public async Task PostChatStream_WithToken_EmitsDeltaAndDoneEvents()
    {
        var token = await LoginAsync();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/chat/stream");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new ChatRequestDto { Message = "Hello" });

        using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/event-stream", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("event: delta", body, StringComparison.Ordinal);
        Assert.Contains("event: done", body, StringComparison.Ordinal);
        Assert.Contains("event: started", body, StringComparison.Ordinal);
        Assert.Contains("Hello", body, StringComparison.Ordinal);
    }

    private async Task<string> LoginAsync()
    {
        var loginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest { Email = "user@demo.local", Password = "User123!" });

        loginResponse.EnsureSuccessStatusCode();
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(login);
        return login.AccessToken;
    }

    internal sealed class StreamingStubAiChatClientFactory : IAiChatClientFactory
    {
        public IAiChatCompletionClient GetClient() => new StreamingStubAiChatCompletionClient();
    }

    private sealed class StreamingStubAiChatCompletionClient : IAiChatCompletionClient
    {
        public Task<ChatCompletionResult> CompleteAsync(
            ChatCompletionRequest request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new ChatCompletionResult(
                "Hello",
                "stub-model",
                LlmProviders.Ollama,
                new TokenUsage(1, 1, 1, 1)));

        public async IAsyncEnumerable<ChatStreamChunk> StreamAsync(
            ChatCompletionRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            yield return new ChatStreamChunk("Hello", IsFinal: false);
            yield return new ChatStreamChunk(
                string.Empty,
                IsFinal: true,
                "stub-model",
                LlmProviders.Ollama,
                new TokenUsage(1, 1, 1, 1));
            await Task.CompletedTask;
        }
    }
}
