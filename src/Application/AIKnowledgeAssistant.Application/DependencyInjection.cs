using AIKnowledgeAssistant.Application.Documents;
using Microsoft.Extensions.DependencyInjection;

namespace AIKnowledgeAssistant.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDocumentService, DocumentService>();
        return services;
    }
}
