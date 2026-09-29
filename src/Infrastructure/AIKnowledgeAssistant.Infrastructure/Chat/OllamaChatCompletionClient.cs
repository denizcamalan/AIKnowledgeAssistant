using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using AIKnowledgeAssistant.Application.Chat;
using AIKnowledgeAssistant.Application.Chat.Tokens;
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
            Format = request.RequestJsonFormat ? "json" : null,
            Messages = ToOllamaMessages(request.Messages),
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

        using (response)
        {
            await EnsureSuccessOrThrowAsync(response, model, cancellationToken);

            var ollamaResponse = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(cancellationToken);
            var content = ollamaResponse?.Message?.Content;
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new AiChatProviderException("Ollama returned an empty assistant message.");
            }

            var usage = BuildTokenUsage(ollamaResponse, request, content.Trim());
            var durationMs = ToDurationMs(ollamaResponse?.TotalDurationNanoseconds);

            return new ChatCompletionResult(content.Trim(), model, LlmProviders.Ollama, usage, durationMs);
        }
    }

    public async IAsyncEnumerable<ChatStreamChunk> StreamAsync(
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (request.RequestJsonFormat)
        {
            throw new AiChatProviderException("Streaming is not supported together with JSON response format.");
        }

        var model = string.IsNullOrWhiteSpace(request.Model) ? _options.Model : request.Model;
        var payload = new OllamaChatRequest
        {
            Model = model,
            Stream = true,
            Messages = ToOllamaMessages(request.Messages),
        };

        HttpResponseMessage response;
        try
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/chat")
            {
                Content = JsonContent.Create(payload),
            };
            response = await _httpClient.SendAsync(
                httpRequest,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
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

        using (response)
        {
            await EnsureSuccessOrThrowAsync(response, model, cancellationToken);

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var line = await reader.ReadLineAsync(cancellationToken);
                if (line is null)
                {
                    break;
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                foreach (var chunk in OllamaStreamLineParser.ParseLine(line, model))
                {
                    yield return chunk;
                }
            }
        }
    }

    private async Task EnsureSuccessOrThrowAsync(
        HttpResponseMessage response,
        string model,
        CancellationToken cancellationToken)
    {
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
    }

    private static TokenUsage BuildTokenUsage(
        OllamaChatResponse? response,
        ChatCompletionRequest request,
        string completionText)
    {
        var estimator = new HeuristicTokenEstimator();
        var promptEstimate = estimator.Estimate(
            string.Join('\n', request.Messages.Select(message => message.Content)));

        return new TokenUsage(
            response?.PromptEvalCount,
            response?.EvalCount,
            promptEstimate,
            estimator.Estimate(completionText));
    }

    private static long? ToDurationMs(long? nanoseconds) =>
        nanoseconds is long value ? value / 1_000_000 : null;

    private static IReadOnlyList<OllamaChatMessage> ToOllamaMessages(IReadOnlyList<ChatMessage> messages) =>
        messages
            .Select(message => new OllamaChatMessage { Role = message.Role, Content = message.Content })
            .ToList();

    private static string TrimForDetail(string value) =>
        value.Length <= 500 ? value : value[..500];

    private sealed class OllamaChatRequest
    {
        [JsonPropertyName("model")]
        public required string Model { get; init; }

        [JsonPropertyName("stream")]
        public bool Stream { get; init; }

        [JsonPropertyName("format")]
        public string? Format { get; init; }

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

        [JsonPropertyName("prompt_eval_count")]
        public int? PromptEvalCount { get; init; }

        [JsonPropertyName("eval_count")]
        public int? EvalCount { get; init; }

        [JsonPropertyName("total_duration")]
        public long? TotalDurationNanoseconds { get; init; }
    }

    private sealed class OllamaAssistantMessage
    {
        [JsonPropertyName("content")]
        public string? Content { get; init; }
    }
}
