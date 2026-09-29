using AIKnowledgeAssistant.Api.Contracts.Auth;
using AIKnowledgeAssistant.Application.Auth;
using Microsoft.AspNetCore.Mvc;

namespace AIKnowledgeAssistant.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> LoginAsync(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password, cancellationToken);
        return Ok(new LoginResponse
        {
            AccessToken = result.AccessToken,
            ExpiresAtUtc = result.ExpiresAtUtc,
            User = ToProfile(result.User),
        });
    }

    private static UserProfileDto ToProfile(AuthenticatedUser user) =>
        new()
        {
            UserId = user.UserId,
            Email = user.Email,
            DisplayName = user.DisplayName,
            Roles = user.Roles,
        };
}
