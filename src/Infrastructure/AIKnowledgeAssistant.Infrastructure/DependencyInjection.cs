using AIKnowledgeAssistant.Application.Auth;
using AIKnowledgeAssistant.Application.Embeddings;
using AIKnowledgeAssistant.Application.Chat;
using AIKnowledgeAssistant.Application.Configuration;
using AIKnowledgeAssistant.Application.Documents;
using AIKnowledgeAssistant.Application.Ingestion;
using AIKnowledgeAssistant.Infrastructure.Auth;
using AIKnowledgeAssistant.Infrastructure.Chat;
using AIKnowledgeAssistant.Infrastructure.Documents;
using AIKnowledgeAssistant.Infrastructure.Embeddings;
using AIKnowledgeAssistant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AIKnowledgeAssistant.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DocumentStorageOptions>(configuration.GetSection(DocumentStorageOptions.SectionName));
        services.Configure<IngestionOptions>(configuration.GetSection(IngestionOptions.SectionName));
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<LlmOptions>(configuration.GetSection(LlmOptions.SectionName));
        services.Configure<EmbeddingOptions>(configuration.GetSection(EmbeddingOptions.SectionName));
        services.AddChatInfrastructure(configuration);
        services.AddEmbeddingInfrastructure(configuration);
        services.AddSingleton<IUserCredentialStore, DemoUserCredentialStore>();
        services.AddSingleton<IAccessTokenFactory, JwtAccessTokenFactory>();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured. Set ConnectionStrings__DefaultConnection or appsettings.");
        }

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql => npgsql.UseVector()));
        services.AddScoped<IDocumentRepository, EfDocumentRepository>();
        services.AddScoped<IDocumentChunkRepository, EfDocumentChunkRepository>();
        services.AddSingleton<IFileStorage, LocalFileStorage>();

        return services;
    }

    private static IServiceCollection AddChatInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var llmOptions = configuration.GetSection(LlmOptions.SectionName).Get<LlmOptions>() ?? new LlmOptions();
        var ollamaBaseUrl = llmOptions.Ollama.BaseUrl.TrimEnd('/') + "/";

        services.AddHttpClient<OllamaChatCompletionClient>(client =>
        {
            client.BaseAddress = new Uri(ollamaBaseUrl);
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        services.AddSingleton<IAiChatClientFactory, AiChatClientFactory>();
        return services;
    }

    private static IServiceCollection AddEmbeddingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var llmOptions = configuration.GetSection(LlmOptions.SectionName).Get<LlmOptions>() ?? new LlmOptions();
        var ollamaBaseUrl = llmOptions.Ollama.BaseUrl.TrimEnd('/') + "/";

        services.AddHttpClient<OllamaEmbeddingClient>(client =>
        {
            client.BaseAddress = new Uri(ollamaBaseUrl);
            client.Timeout = TimeSpan.FromMinutes(2);
        });

        services.AddSingleton<IEmbeddingClientFactory, EmbeddingClientFactory>();
        return services;
    }
}
