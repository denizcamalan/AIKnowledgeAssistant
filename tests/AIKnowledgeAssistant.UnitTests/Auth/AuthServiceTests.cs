using AIKnowledgeAssistant.Application.Auth;
using Microsoft.AspNetCore.Identity;

namespace AIKnowledgeAssistant.UnitTests.Auth;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsInvalidCredentialsException()
    {
        var service = new AuthService(new FakeUserStore(), new FakeTokenFactory());

        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            service.LoginAsync("user@demo.local", "bad", CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_ValidPassword_ReturnsToken()
    {
        var service = new AuthService(new FakeUserStore(), new FakeTokenFactory());

        var result = await service.LoginAsync("user@demo.local", "secret", CancellationToken.None);

        Assert.Equal("token-uid", result.AccessToken);
        Assert.Equal("user@demo.local", result.User.Email);
        Assert.Equal(["User"], result.User.Roles);
    }

    private sealed class FakeUserStore : IUserCredentialStore
    {
        private static readonly StoredUser DemoUser = CreateUser();

        public Task<StoredUser?> FindByEmailAsync(string email, CancellationToken cancellationToken)
        {
            if (!string.Equals(email, DemoUser.Email, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<StoredUser?>(null);
            }

            return Task.FromResult<StoredUser?>(DemoUser);
        }

        private static StoredUser CreateUser()
        {
            var hasher = new PasswordHasher<StoredUser>();
            var seed = new StoredUser("uid", "user@demo.local", "User", string.Empty, ["User"]);
            return seed with { PasswordHash = hasher.HashPassword(seed, "secret") };
        }
    }

    private sealed class FakeTokenFactory : IAccessTokenFactory
    {
        public (string Token, DateTimeOffset ExpiresAtUtc) CreateAccessToken(AuthenticatedUser user) =>
            ($"token-{user.UserId}", DateTimeOffset.UtcNow.AddHours(1));
    }
}
