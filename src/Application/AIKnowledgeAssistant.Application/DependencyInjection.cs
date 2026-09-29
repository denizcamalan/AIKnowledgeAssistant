using AIKnowledgeAssistant.Application.Auth;
using AIKnowledgeAssistant.Application.Chat;
using AIKnowledgeAssistant.Application.Chat.PromptLab;
using AIKnowledgeAssistant.Application.Chat.Tokens;
using AIKnowledgeAssistant.Application.Documents;
using Microsoft.Extensions.DependencyInjection;

namespace AIKnowledgeAssistant.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAiChatService, ChatService>();
        services.AddScoped<IPromptLabService, PromptLabService>();
        services.AddSingleton<ITokenEstimator, HeuristicTokenEstimator>();
        services.AddSingleton<IContextTruncationService, HeadTailContextTruncationService>();
        services.AddScoped<ITokenContextLabService, TokenContextLabService>();
        return services;
    }
}
