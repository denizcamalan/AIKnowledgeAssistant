namespace AIKnowledgeAssistant.Api.LifetimeLab;

public interface ILifetimeInstanceMarker
{
    Guid InstanceId { get; }

    string LifetimeName { get; }
}

public interface ITransientLifetimeMarker : ILifetimeInstanceMarker;

public interface IScopedLifetimeMarker : ILifetimeInstanceMarker;

public interface ISingletonLifetimeMarker : ILifetimeInstanceMarker;
