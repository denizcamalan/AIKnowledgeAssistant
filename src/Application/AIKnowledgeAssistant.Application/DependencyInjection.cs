using AIKnowledgeAssistant.Application.Auth;
using AIKnowledgeAssistant.Application.Chat;
using AIKnowledgeAssistant.Application.Chat.PromptLab;
using AIKnowledgeAssistant.Application.Chat.Tokens;
using AIKnowledgeAssistant.Application.Documents;
using AIKnowledgeAssistant.Application.Documents.StructuredOutput;
using AIKnowledgeAssistant.Application.Ingestion;
using Microsoft.Extensions.DependencyInjection;

namespace AIKnowledgeAssistant.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IDocumentInsightService, DocumentInsightService>();
        services.AddSingleton<IStructuredLlmJsonParser, StructuredLlmJsonParser>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAiChatService, ChatService>();
        services.AddScoped<IChatStreamService, ChatStreamService>();
        services.AddSingleton<IChatStreamSessionRegistry, ChatStreamSessionRegistry>();
        services.AddScoped<IPromptLabService, PromptLabService>();
        services.AddSingleton<ITokenEstimator, HeuristicTokenEstimator>();
        services.AddSingleton<IContextTruncationService, HeadTailContextTruncationService>();
        services.AddScoped<ITokenContextLabService, TokenContextLabService>();
        services.AddScoped<IDocumentIngestionPipeline, DocumentIngestionPipeline>();
        return services;
    }
}
