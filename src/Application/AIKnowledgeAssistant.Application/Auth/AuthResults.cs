namespace AIKnowledgeAssistant.Application.Auth;

public sealed record AuthenticatedUser(
    string UserId,
    string Email,
    string DisplayName,
    IReadOnlyList<string> Roles);

public sealed record LoginSuccessResult(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    AuthenticatedUser User);

public sealed class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException()
        : base("Invalid email or password.")
    {
    }
}
