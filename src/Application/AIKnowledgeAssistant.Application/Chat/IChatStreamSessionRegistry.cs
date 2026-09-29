namespace AIKnowledgeAssistant.Application.Chat;

public interface IChatStreamSessionRegistry
{
    ChatStreamSession StartSession(string userId);

    bool TryStopSession(string userId, Guid streamId);

    void CompleteSession(Guid streamId);
}

public sealed class ChatStreamSession : IDisposable
{
    public ChatStreamSession(Guid streamId, CancellationTokenSource cancellationTokenSource)
    {
        StreamId = streamId;
        CancellationTokenSource = cancellationTokenSource;
    }

    public Guid StreamId { get; }

    public CancellationTokenSource CancellationTokenSource { get; }

    public CancellationToken Token => CancellationTokenSource.Token;

    public void Dispose() => CancellationTokenSource.Dispose();
}

public sealed class ChatStreamSessionRegistry : IChatStreamSessionRegistry
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<Guid, (string UserId, CancellationTokenSource Cts)> _sessions =
        new();

    public ChatStreamSession StartSession(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        var streamId = Guid.NewGuid();
        var cts = new CancellationTokenSource();
        _sessions[streamId] = (userId, cts);
        return new ChatStreamSession(streamId, cts);
    }

    public bool TryStopSession(string userId, Guid streamId)
    {
        if (!_sessions.TryGetValue(streamId, out var entry) ||
            !string.Equals(entry.UserId, userId, StringComparison.Ordinal))
        {
            return false;
        }

        entry.Cts.Cancel();
        _sessions.TryRemove(streamId, out _);
        return true;
    }

    public void CompleteSession(Guid streamId) =>
        _sessions.TryRemove(streamId, out _);
}
