using AIKnowledgeAssistant.Application.Chat;
using AIKnowledgeAssistant.Application.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AIKnowledgeAssistant.Infrastructure.Chat;

public sealed class AiChatClientFactory : IAiChatClientFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly LlmOptions _options;

    public AiChatClientFactory(IServiceProvider serviceProvider, IOptions<LlmOptions> options)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
    }

    public IAiChatCompletionClient GetClient()
    {
        if (string.Equals(_options.Provider, LlmProviders.Ollama, StringComparison.OrdinalIgnoreCase))
        {
            return _serviceProvider.GetRequiredService<OllamaChatCompletionClient>();
        }

        throw new InvalidOperationException(
            $"LLM provider '{_options.Provider}' is not registered. Supported providers: {LlmProviders.Ollama}.");
    }
}
