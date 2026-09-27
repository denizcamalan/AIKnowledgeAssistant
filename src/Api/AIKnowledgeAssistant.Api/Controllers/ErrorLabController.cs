using Microsoft.AspNetCore.Mvc;

namespace AIKnowledgeAssistant.Api.Controllers;

/// <summary>
/// Development-only endpoint to verify global 500 handling (P1-05).
/// </summary>
[ApiController]
[Route("api/labs/errors")]
public sealed class ErrorLabController : ControllerBase
{
    [HttpGet("unhandled")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public IActionResult ThrowUnhandled() =>
        throw new InvalidOperationException("Laboratory unhandled exception.");
}
