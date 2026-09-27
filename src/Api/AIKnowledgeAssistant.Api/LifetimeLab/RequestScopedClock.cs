namespace AIKnowledgeAssistant.Api.LifetimeLab;

/// <summary>
/// Scoped service representing per-request state (analogous to DbContext scope).
/// </summary>
public sealed class RequestScopedClock
{
    public Guid RequestScopeId { get; } = Guid.NewGuid();

    public DateTimeOffset CreatedAtUtc { get; } = DateTimeOffset.UtcNow;
}
