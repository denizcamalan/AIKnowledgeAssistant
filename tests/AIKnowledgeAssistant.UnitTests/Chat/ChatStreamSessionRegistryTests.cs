using AIKnowledgeAssistant.Application.Chat;

namespace AIKnowledgeAssistant.UnitTests.Chat;

public sealed class ChatStreamSessionRegistryTests
{
    [Fact]
    public void TryStopSession_WithMatchingUser_CancelsSession()
    {
        var registry = new ChatStreamSessionRegistry();
        using var session = registry.StartSession("user-1");

        var stopped = registry.TryStopSession("user-1", session.StreamId);

        Assert.True(stopped);
        Assert.True(session.Token.IsCancellationRequested);
    }

    [Fact]
    public void TryStopSession_WithDifferentUser_ReturnsFalse()
    {
        var registry = new ChatStreamSessionRegistry();
        using var session = registry.StartSession("user-1");

        var stopped = registry.TryStopSession("user-2", session.StreamId);

        Assert.False(stopped);
        Assert.False(session.Token.IsCancellationRequested);
    }
}
