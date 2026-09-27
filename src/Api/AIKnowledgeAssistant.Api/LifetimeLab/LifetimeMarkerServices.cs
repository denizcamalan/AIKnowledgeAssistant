namespace AIKnowledgeAssistant.Api.LifetimeLab;

internal sealed class TransientLifetimeMarker : ITransientLifetimeMarker
{
    public Guid InstanceId { get; } = Guid.NewGuid();

    public string LifetimeName => "Transient";
}

internal sealed class ScopedLifetimeMarker : IScopedLifetimeMarker
{
    public Guid InstanceId { get; } = Guid.NewGuid();

    public string LifetimeName => "Scoped";
}

internal sealed class SingletonLifetimeMarker : ISingletonLifetimeMarker
{
    public Guid InstanceId { get; } = Guid.NewGuid();

    public string LifetimeName => "Singleton";
}
