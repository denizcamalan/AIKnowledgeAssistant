using AIKnowledgeAssistant.Api.AsyncLab;
using Microsoft.AspNetCore.Mvc;

namespace AIKnowledgeAssistant.Api.Controllers;

/// <summary>
/// Learning endpoint that contrasts I/O-bound and CPU-bound async work (P1-06).
/// </summary>
[ApiController]
[Route("api/labs/async")]
public sealed class AsyncLabController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(AsyncLabResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AsyncLabResponse>> Get(
        [FromQuery] int ioDelayMs = 25,
        CancellationToken cancellationToken = default)
    {
        var ioBound = await AsyncWork.RunIoBoundAsync(ioDelayMs, cancellationToken);
        var cpuBound = await AsyncWork.RunCpuBoundAsync(cancellationToken);

        return Ok(new AsyncLabResponse(ioBound, cpuBound, AsyncWork.Notes));
    }
}

public sealed record AsyncLabResponse(
    AsyncWorkSample IoBound,
    AsyncWorkSample CpuBound,
    IReadOnlyList<string> Notes);
