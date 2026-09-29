using AIKnowledgeAssistant.Application.Auth;
using Microsoft.AspNetCore.Identity;

namespace AIKnowledgeAssistant.Infrastructure.Auth;

/// <summary>Demo-only in-memory users for P2-01. Replace with persistent identity in a later phase.</summary>
internal sealed class DemoUserCredentialStore : IUserCredentialStore
{
    private static readonly IReadOnlyList<StoredUser> Users = BuildUsers();

    public Task<StoredUser?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var match = Users.FirstOrDefault(u =>
            string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(match);
    }

    private static IReadOnlyList<StoredUser> BuildUsers()
    {
        var hasher = new PasswordHasher<StoredUser>();
        return
        [
            HashUser(hasher, "11111111-1111-1111-1111-111111111111", "admin@demo.local", "Demo Admin", "Admin123!", ["Admin", "User"]),
            HashUser(hasher, "22222222-2222-2222-2222-222222222222", "user@demo.local", "Demo User", "User123!", ["User"]),
        ];
    }

    private static StoredUser HashUser(
        PasswordHasher<StoredUser> hasher,
        string userId,
        string email,
        string displayName,
        string plainPassword,
        string[] roles)
    {
        var seed = new StoredUser(userId, email, displayName, string.Empty, roles);
        var hash = hasher.HashPassword(seed, plainPassword);
        return seed with { PasswordHash = hash };
    }
}
