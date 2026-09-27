namespace AIKnowledgeAssistant.Api.LifetimeLab;

public static class LifetimeLabServiceCollectionExtensions
{
    public static IServiceCollection AddLifetimeLab(this IServiceCollection services)
    {
        services.AddTransient<ITransientLifetimeMarker, TransientLifetimeMarker>();
        services.AddScoped<IScopedLifetimeMarker, ScopedLifetimeMarker>();
        services.AddSingleton<ISingletonLifetimeMarker, SingletonLifetimeMarker>();
        services.AddScoped<RequestScopedClock>();
        return services;
    }

    /// <summary>
    /// Registers singleton → scoped captive dependency for validation tests only.
    /// </summary>
    public static IServiceCollection AddCaptiveDependencyAntiPattern(this IServiceCollection services)
    {
        services.AddScoped<RequestScopedClock>();
        services.AddSingleton<CaptiveDependencyConsumer>();
        return services;
    }
}
