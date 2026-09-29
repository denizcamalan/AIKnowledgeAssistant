using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AIKnowledgeAssistant.Api.Contracts.Auth;
using AIKnowledgeAssistant.Api.Contracts.PromptLab;
using AIKnowledgeAssistant.Application.Chat;
using AIKnowledgeAssistant.Application.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AIKnowledgeAssistant.IntegrationTests;

public sealed class PromptLabEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PromptLabEndpointsTests(CustomWebApplicationFactory factory)
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
    public async Task Compare_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/labs/prompts/compare",
            new PromptLabCompareRequestDto { Question = "Test?" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Compare_WithToken_ReturnsThreeRecordedVariants()
    {
        var token = await LoginAsync();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/labs/prompts/compare");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new PromptLabCompareRequestDto
        {
            Question = "Haftada kaç gün uzaktan çalışabilirim?",
        });

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PromptLabCompareResponseDto>();
        Assert.NotNull(body);
        Assert.Equal(3, body.Variants.Count);
        Assert.Equal(["baseline", "constrained", "grounded"], body.Variants.Select(variant => variant.Variant).ToList());
        Assert.True(body.EvaluationCriteria.Count >= 3);
        Assert.All(body.Variants, variant => Assert.False(string.IsNullOrWhiteSpace(variant.AssistantMessage)));
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
                LlmProviders.Ollama));
        }
    }
}
