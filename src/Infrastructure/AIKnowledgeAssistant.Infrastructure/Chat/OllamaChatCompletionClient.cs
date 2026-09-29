using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AIKnowledgeAssistant.Application.Chat;
using AIKnowledgeAssistant.Application.Configuration;
using Microsoft.Extensions.Options;

namespace AIKnowledgeAssistant.Infrastructure.Chat;

public sealed class OllamaChatCompletionClient : IAiChatCompletionClient
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;

    public OllamaChatCompletionClient(HttpClient httpClient, IOptions<LlmOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value.Ollama;
    }

    public async Task<ChatCompletionResult> CompleteAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken)
    {
        var model = string.IsNullOrWhiteSpace(request.Model) ? _options.Model : request.Model;
        var payload = new OllamaChatRequest
        {
            Model = model,
            Stream = false,
            Messages = request.Messages
                .Select(message => new OllamaChatMessage { Role = message.Role, Content = message.Content })
                .ToList(),
        };

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync("api/chat", payload, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            throw new AiChatProviderException(
                "Could not reach the Ollama server. Ensure Ollama is running and Llm:Ollama:BaseUrl is correct.",
                exception);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new AiChatProviderException("The Ollama request timed out.", exception);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new AiChatProviderException(
                $"Ollama model '{model}' was not found. Run `ollama pull {model}` and try again.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new AiChatProviderException(
                $"Ollama returned {(int)response.StatusCode}: {TrimForDetail(body)}");
        }

        var ollamaResponse = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(cancellationToken);
        var content = ollamaResponse?.Message?.Content;
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new AiChatProviderException("Ollama returned an empty assistant message.");
        }

        return new ChatCompletionResult(content.Trim(), model, LlmProviders.Ollama);
    }

    private static string TrimForDetail(string value) =>
        value.Length <= 500 ? value : value[..500];

    private sealed class OllamaChatRequest
    {
        [JsonPropertyName("model")]
        public required string Model { get; init; }

        [JsonPropertyName("stream")]
        public bool Stream { get; init; }

        [JsonPropertyName("messages")]
        public required IReadOnlyList<OllamaChatMessage> Messages { get; init; }
    }

    private sealed class OllamaChatMessage
    {
        [JsonPropertyName("role")]
        public required string Role { get; init; }

        [JsonPropertyName("content")]
        public required string Content { get; init; }
    }

    private sealed class OllamaChatResponse
    {
        [JsonPropertyName("message")]
        public OllamaAssistantMessage? Message { get; init; }
    }

    private sealed class OllamaAssistantMessage
    {
        [JsonPropertyName("content")]
        public string? Content { get; init; }
    }
}
