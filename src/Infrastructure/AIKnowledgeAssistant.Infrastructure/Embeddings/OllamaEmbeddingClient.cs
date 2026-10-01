using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AIKnowledgeAssistant.Application.Configuration;
using AIKnowledgeAssistant.Application.Embeddings;
using Microsoft.Extensions.Options;

namespace AIKnowledgeAssistant.Infrastructure.Embeddings;

public sealed class OllamaEmbeddingClient : IEmbeddingClient
{
    private readonly HttpClient _httpClient;
    private readonly EmbeddingOptions _options;

    public OllamaEmbeddingClient(HttpClient httpClient, IOptions<EmbeddingOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<float[]>> EmbedAsync(
        IReadOnlyList<string> inputs,
        CancellationToken cancellationToken)
    {
        if (inputs.Count == 0)
        {
            return [];
        }

        var payload = new OllamaEmbedRequest
        {
            Model = _options.Model,
            Input = inputs.Count == 1 ? inputs[0] : inputs.ToArray(),
        };

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync("api/embed", payload, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            throw new AiEmbeddingProviderException(
                "Could not reach the Ollama server for embeddings. Ensure Ollama is running and Embeddings:Model is pulled.",
                exception);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new AiEmbeddingProviderException("The Ollama embedding request timed out.", exception);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new AiEmbeddingProviderException(
                    $"Ollama embedding request failed ({(int)response.StatusCode}): {body}");
            }

            var embedResponse = await response.Content.ReadFromJsonAsync<OllamaEmbedResponse>(cancellationToken);
            var embeddings = embedResponse?.Embeddings;
            if (embeddings is null || embeddings.Count == 0)
            {
                throw new AiEmbeddingProviderException("Ollama returned no embeddings.");
            }

            if (embeddings.Count != inputs.Count)
            {
                throw new AiEmbeddingProviderException(
                    $"Ollama returned {embeddings.Count} embeddings for {inputs.Count} inputs.");
            }

            return embeddings;
        }
    }

    private sealed class OllamaEmbedRequest
    {
        [JsonPropertyName("model")]
        public required string Model { get; init; }

        [JsonPropertyName("input")]
        public required object Input { get; init; }
    }

    private sealed class OllamaEmbedResponse
    {
        [JsonPropertyName("embeddings")]
        public List<float[]> Embeddings { get; init; } = [];
    }
}
