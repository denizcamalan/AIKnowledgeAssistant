namespace AIKnowledgeAssistant.Application.Chat;

/// <summary>
/// Selects the configured LLM provider implementation (factory pattern for model backends).
/// </summary>
public interface IAiChatClientFactory
{
    IAiChatCompletionClient GetClient();
}
