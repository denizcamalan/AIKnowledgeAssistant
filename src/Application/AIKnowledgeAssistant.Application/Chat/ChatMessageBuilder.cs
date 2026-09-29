namespace AIKnowledgeAssistant.Application.Chat;

internal static class ChatMessageBuilder
{
    public static IReadOnlyList<ChatMessage> BuildMessages(ChatPrompt prompt)
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

    public static string SerializeForEstimate(IReadOnlyList<ChatMessage> messages) =>
        string.Join('\n', messages.Select(message => message.Content));
}
