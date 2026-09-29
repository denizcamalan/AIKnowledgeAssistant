namespace AIKnowledgeAssistant.Application.Auth;

public interface IUserCredentialStore
{
    Task<StoredUser?> FindByEmailAsync(string email, CancellationToken cancellationToken);
}

public sealed record StoredUser(
    string UserId,
    string Email,
    string DisplayName,
    string PasswordHash,
    IReadOnlyList<string> Roles);
