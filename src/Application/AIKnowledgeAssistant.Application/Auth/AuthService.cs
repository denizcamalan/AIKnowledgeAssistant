using Microsoft.AspNetCore.Identity;

namespace AIKnowledgeAssistant.Application.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IUserCredentialStore _users;
    private readonly IAccessTokenFactory _tokens;
    private readonly PasswordHasher<StoredUser> _passwordHasher = new();

    public AuthService(IUserCredentialStore users, IAccessTokenFactory tokens)
    {
        _users = users;
        _tokens = tokens;
    }

    public async Task<LoginSuccessResult> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidCredentialsException();
        }

        var stored = await _users.FindByEmailAsync(email.Trim(), cancellationToken);
        if (stored is null)
        {
            throw new InvalidCredentialsException();
        }

        var verification = _passwordHasher.VerifyHashedPassword(stored, stored.PasswordHash, password);
        if (verification is PasswordVerificationResult.Failed)
        {
            throw new InvalidCredentialsException();
        }

        var user = new AuthenticatedUser(stored.UserId, stored.Email, stored.DisplayName, stored.Roles);
        var (token, expiresAt) = _tokens.CreateAccessToken(user);
        return new LoginSuccessResult(token, expiresAt, user);
    }
}
