using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AIKnowledgeAssistant.Api.Contracts.Auth;
using AIKnowledgeAssistant.Api.Contracts.Chat;
using AIKnowledgeAssistant.Application.Chat;
using AIKnowledgeAssistant.Application.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AIKnowledgeAssistant.IntegrationTests;

public sealed class ChatEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ChatEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IAiChatClientFactory>();
                services.AddSingleton<IAiChatClientFactory, StubAiChatClientFactory>();
            });
        }).CreateClient();
    }

    [Fact]
    public async Task PostChat_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/chat",
            new ChatRequestDto { Message = "Hello" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostChat_WithToken_ReturnsAssistantMessage()
    {
        var token = await LoginAsync();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/chat");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new ChatRequestDto { Message = "What is RAG?" });

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ChatResponseDto>();
        Assert.NotNull(body);
        Assert.Equal("Stubbed assistant reply.", body.Message);
        Assert.Equal(LlmProviders.Ollama, body.Provider);
        Assert.Equal("stub-model", body.Model);
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

    private sealed class StubAiChatClientFactory : IAiChatClientFactory
    {
        public IAiChatCompletionClient GetClient() => new StubAiChatCompletionClient();
    }

    private sealed class StubAiChatCompletionClient : IAiChatCompletionClient
    {
        public Task<ChatCompletionResult> CompleteAsync(
            ChatCompletionRequest request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new ChatCompletionResult(
                "Stubbed assistant reply.",
                "stub-model",
                LlmProviders.Ollama));
    }
}
