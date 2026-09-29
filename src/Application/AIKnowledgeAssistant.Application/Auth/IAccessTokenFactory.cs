namespace AIKnowledgeAssistant.Application.Auth;

public interface IAccessTokenFactory
{
    (string Token, DateTimeOffset ExpiresAtUtc) CreateAccessToken(AuthenticatedUser user);
}
