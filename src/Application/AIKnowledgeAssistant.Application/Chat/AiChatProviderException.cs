namespace AIKnowledgeAssistant.Application.Chat;

public sealed class AiChatProviderException : Exception
{
    public AiChatProviderException(string message)
        : base(message)
    {
    }

    public AiChatProviderException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
