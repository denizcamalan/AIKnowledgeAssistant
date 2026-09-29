namespace AIKnowledgeAssistant.Application.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "AIKnowledgeAssistant";

    public string Audience { get; set; } = "AIKnowledgeAssistant.Api";

    /// <summary>Symmetric signing key (min 32 bytes for HS256). Override via user secrets or environment in production.</summary>
    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenLifetimeMinutes { get; set; } = 60;
}
