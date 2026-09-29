using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AIKnowledgeAssistant.Api.Contracts.Auth;
using AIKnowledgeAssistant.Api.Contracts.Tokens;
using AIKnowledgeAssistant.Application.Chat;
using AIKnowledgeAssistant.Application.Chat.Tokens;
using AIKnowledgeAssistant.Application.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AIKnowledgeAssistant.IntegrationTests;

public sealed class TokenLabEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TokenLabEndpointsTests(CustomWebApplicationFactory factory)
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
    public async Task ContextExperiment_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/labs/tokens/context-experiment",
            new TokenContextExperimentRequestDto { Question = "Test?" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ContextExperiment_WithToken_ReturnsThreeScenarios()
    {
        var token = await LoginAsync();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/labs/tokens/context-experiment");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new TokenContextExperimentRequestDto
        {
            Question = "Haftada kaç gün uzaktan çalışabilirim?",
        });

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<TokenContextExperimentResponseDto>();
        Assert.NotNull(body);
        Assert.Equal(3, body.Scenarios.Count);
        Assert.Contains(body.Scenarios, scenario => scenario.Scenario == "long-context-truncated");
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
        private int _calls;

        public IAiChatCompletionClient GetClient() => new StubAiChatCompletionClient(() => Interlocked.Increment(ref _calls));
    }

    private sealed class StubAiChatCompletionClient(Func<int> nextCall) : IAiChatCompletionClient
    {
        public Task<ChatCompletionResult> CompleteAsync(
            ChatCompletionRequest request,
            CancellationToken cancellationToken)
        {
            var call = nextCall();
            return Task.FromResult(new ChatCompletionResult(
                $"stub-{call}",
                "stub-model",
                LlmProviders.Ollama,
                new TokenUsage(call * 300, 15, call * 250, 15),
                ProviderDurationMs: call * 5));
        }
    }
}
