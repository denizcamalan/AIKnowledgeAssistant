namespace AIKnowledgeAssistant.Api.LifetimeLab;

/// <summary>
/// Anti-pattern: singleton holding a scoped dependency (captive dependency).
/// Used only in unit tests — do not register in the application container.
/// </summary>
public sealed class CaptiveDependencyConsumer
{
    public CaptiveDependencyConsumer(RequestScopedClock requestScopedClock) =>
        RequestScopedClock = requestScopedClock;

    public RequestScopedClock RequestScopedClock { get; }
}
