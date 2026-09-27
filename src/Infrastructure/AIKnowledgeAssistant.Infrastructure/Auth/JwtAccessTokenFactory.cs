using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AIKnowledgeAssistant.Application.Auth;
using AIKnowledgeAssistant.Application.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AIKnowledgeAssistant.Infrastructure.Auth;

internal sealed class JwtAccessTokenFactory : IAccessTokenFactory
{
    private readonly JwtOptions _options;
    private readonly JwtSecurityTokenHandler _handler = new();

    public JwtAccessTokenFactory(IOptions<JwtOptions> options) => _options = options.Value;

    public (string Token, DateTimeOffset ExpiresAtUtc) CreateAccessToken(AuthenticatedUser user)
    {
        if (string.IsNullOrWhiteSpace(_options.SigningKey) || _options.SigningKey.Length < 32)
        {
            throw new InvalidOperationException(
                "Jwt:SigningKey must be at least 32 characters. Set Jwt__SigningKey for this environment.");
        }

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_options.AccessTokenLifetimeMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("display_name", user.DisplayName),
        };

        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (_handler.WriteToken(token), expiresAt);
    }
}
