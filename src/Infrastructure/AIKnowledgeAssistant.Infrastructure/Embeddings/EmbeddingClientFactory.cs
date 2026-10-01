using AIKnowledgeAssistant.Application.Configuration;
using AIKnowledgeAssistant.Application.Embeddings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AIKnowledgeAssistant.Infrastructure.Embeddings;

public sealed class EmbeddingClientFactory : IEmbeddingClientFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly EmbeddingOptions _options;

    public EmbeddingClientFactory(IServiceProvider serviceProvider, IOptions<EmbeddingOptions> options)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
    }

    public IEmbeddingClient GetClient()
    {
        if (string.Equals(_options.Provider, LlmProviders.Ollama, StringComparison.OrdinalIgnoreCase))
        {
            return _serviceProvider.GetRequiredService<OllamaEmbeddingClient>();
        }

        throw new InvalidOperationException(
            $"Embedding provider '{_options.Provider}' is not registered. Supported providers: {LlmProviders.Ollama}.");
    }
}
