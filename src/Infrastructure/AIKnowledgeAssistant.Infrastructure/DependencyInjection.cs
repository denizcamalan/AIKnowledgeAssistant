using Microsoft.Extensions.DependencyInjection;

namespace AIKnowledgeAssistant.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Persistence, LLM clients, and messaging registrations land in later phases.
        return services;
    }
}
