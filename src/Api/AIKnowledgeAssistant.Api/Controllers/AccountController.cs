using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AIKnowledgeAssistant.Api.Contracts.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIKnowledgeAssistant.Api.Controllers;

[ApiController]
[Route("api/account")]
[Authorize]
public sealed class AccountController : ControllerBase
{
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<UserProfileDto> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var email = User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Email);
        var displayName = User.FindFirstValue("display_name");

        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized();
        }

        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).Distinct().ToList();
        return Ok(new UserProfileDto
        {
            UserId = userId,
            Email = email,
            DisplayName = displayName ?? email,
            Roles = roles,
        });
    }
}
