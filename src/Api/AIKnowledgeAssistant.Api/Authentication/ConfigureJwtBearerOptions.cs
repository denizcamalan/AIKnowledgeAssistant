using System.Text;
using AIKnowledgeAssistant.Application.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AIKnowledgeAssistant.Api.Authentication;

internal sealed class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly JwtOptions _jwt;

    public ConfigureJwtBearerOptions(IOptions<JwtOptions> jwt) => _jwt = jwt.Value;

    public void Configure(JwtBearerOptions options) => Configure(JwtBearerDefaults.AuthenticationScheme, options);

    public void Configure(string? name, JwtBearerOptions options)
    {
        if (name != JwtBearerDefaults.AuthenticationScheme)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_jwt.SigningKey) || _jwt.SigningKey.Length < 32)
        {
            throw new InvalidOperationException(
                "Jwt:SigningKey must be at least 32 characters. Set Jwt__SigningKey for this environment.");
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    }
}
