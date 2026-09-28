using AIKnowledgeAssistant.Application.Auth;
using AIKnowledgeAssistant.Application.Configuration;
using AIKnowledgeAssistant.Application.Documents;
using AIKnowledgeAssistant.Infrastructure.Auth;
using AIKnowledgeAssistant.Infrastructure.Documents;
using AIKnowledgeAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AIKnowledgeAssistant.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DocumentStorageOptions>(configuration.GetSection(DocumentStorageOptions.SectionName));
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<IUserCredentialStore, DemoUserCredentialStore>();
        services.AddSingleton<IAccessTokenFactory, JwtAccessTokenFactory>();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured. Set ConnectionStrings__DefaultConnection or appsettings.");
        }

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IDocumentRepository, EfDocumentRepository>();
        services.AddSingleton<IFileStorage, LocalFileStorage>();

        return services;
    }
}
