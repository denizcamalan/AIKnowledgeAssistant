using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using AIKnowledgeAssistant.Api.Contracts.Auth;
using AIKnowledgeAssistant.Api.Contracts.Chat;
using AIKnowledgeAssistant.Application.Chat;
using AIKnowledgeAssistant.Application.Chat.Tokens;
using AIKnowledgeAssistant.Application.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AIKnowledgeAssistant.IntegrationTests;

public sealed class ChatStreamStopEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ChatStreamStopEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IAiChatClientFactory>();
                services.AddSingleton<IAiChatClientFactory, SlowStreamingStubFactory>();
            });
        }).CreateClient();
    }

    [Fact]
    public async Task StopStream_CancelsActiveSession()
    {
        var token = await LoginAsync();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/chat/stream");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new ChatRequestDto { Message = "Slow" });

        using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        await using var bodyStream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(bodyStream);
        var sseBuffer = new StringBuilder();
        Guid? streamId = null;

        while (streamId is null)
        {
            var line = await reader.ReadLineAsync();
            if (line is null)
            {
                break;
            }

            sseBuffer.AppendLine(line);
            if (sseBuffer.ToString().Contains("\n\n", StringComparison.Ordinal))
            {
                streamId = ExtractStreamId(sseBuffer.ToString());
                sseBuffer.Clear();
            }
        }

        Assert.NotNull(streamId);

        using var stopRequest = new HttpRequestMessage(HttpMethod.Post, "/api/chat/stream/stop");
        stopRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        stopRequest.Content = JsonContent.Create(new ChatStreamStopRequestDto { StreamId = streamId.Value });

        var stopResponse = await _client.SendAsync(stopRequest);
        Assert.Equal(HttpStatusCode.NoContent, stopResponse.StatusCode);

        var remainder = await reader.ReadToEndAsync();
        Assert.Contains("event: stopped", remainder, StringComparison.Ordinal);
    }

    private static Guid? ExtractStreamId(string sseBody)
    {
        var startedIndex = sseBody.IndexOf("event: started", StringComparison.Ordinal);
        if (startedIndex < 0)
        {
            return null;
        }

        var dataIndex = sseBody.IndexOf("data:", startedIndex, StringComparison.Ordinal);
        if (dataIndex < 0)
        {
            return null;
        }

        var jsonStart = dataIndex + "data:".Length;
        var jsonEnd = sseBody.IndexOf('\n', jsonStart);
        var json = sseBody[jsonStart..jsonEnd].Trim();
        using var document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty("streamId").GetGuid();
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

    private sealed class SlowStreamingStubFactory : IAiChatClientFactory
    {
        public IAiChatCompletionClient GetClient() => new SlowStreamingStubClient();
    }

    private sealed class SlowStreamingStubClient : IAiChatCompletionClient
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
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }

            yield return new ChatStreamChunk(string.Empty, IsFinal: true, "stub-model", LlmProviders.Ollama, new TokenUsage(1, 1, 1, 1));
        }
    }
}
