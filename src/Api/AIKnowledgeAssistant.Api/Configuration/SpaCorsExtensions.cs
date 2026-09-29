namespace AIKnowledgeAssistant.Api.Configuration;

public static class SpaCorsExtensions
{
    public const string PolicyName = "Spa";

    public static IServiceCollection AddSpaCors(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>();
        var origins = settings?.Origins is { Length: > 0 } configured
            ? configured
            : ["http://localhost:5173"];

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
                policy.WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });

        return services;
    }
}
