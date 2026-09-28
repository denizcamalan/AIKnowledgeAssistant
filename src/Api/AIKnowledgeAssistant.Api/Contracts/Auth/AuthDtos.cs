using System.ComponentModel.DataAnnotations;

namespace AIKnowledgeAssistant.Api.Contracts.Auth;

public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginResponse
{
    public required string AccessToken { get; init; }

    public required DateTimeOffset ExpiresAtUtc { get; init; }

    public required UserProfileDto User { get; init; }
}

public sealed class UserProfileDto
{
    public required string UserId { get; init; }

    public required string Email { get; init; }

    public required string DisplayName { get; init; }

    public required IReadOnlyList<string> Roles { get; init; }
}
