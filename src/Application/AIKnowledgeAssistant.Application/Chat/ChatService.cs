namespace AIKnowledgeAssistant.Application.Chat;

public sealed class ChatService : IAiChatService
{
    private readonly IAiChatClientFactory _clientFactory;

    public ChatService(IAiChatClientFactory clientFactory) => _clientFactory = clientFactory;

    public async Task<ChatReply> CompleteAsync(ChatPrompt prompt, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(prompt.Message))
        {
            throw new ArgumentException("Message is required.", nameof(prompt));
        }

        var client = _clientFactory.GetClient();
        var messages = BuildMessages(prompt);
        var completion = await client.CompleteAsync(
            new ChatCompletionRequest(messages, Model: string.Empty),
            cancellationToken);

        return new ChatReply(completion.Content, completion.Model, completion.Provider);
    }

    private static IReadOnlyList<ChatMessage> BuildMessages(ChatPrompt prompt)
    {
        var messages = new List<ChatMessage>();
        if (!string.IsNullOrWhiteSpace(prompt.SystemMessage))
        {
            messages.Add(new ChatMessage("system", prompt.SystemMessage.Trim()));
        }

        if (prompt.FewShotExamples is { Count: > 0 })
        {
            messages.AddRange(prompt.FewShotExamples);
        }

        messages.Add(new ChatMessage("user", prompt.Message.Trim()));
        return messages;
    }
}
