using AIKnowledgeAssistant.Api.LifetimeLab;
using Microsoft.AspNetCore.Mvc;

namespace AIKnowledgeAssistant.Api.Controllers;

/// <summary>
/// Development-only lab for observing DI lifetimes within and across HTTP requests.
/// </summary>
[ApiController]
[Route("api/labs/di")]
public sealed class DiLifetimeLabController : ControllerBase
{
    private readonly ITransientLifetimeMarker _transientFromConstructor;
    private readonly IScopedLifetimeMarker _scopedFromConstructor;
    private readonly ISingletonLifetimeMarker _singletonFromConstructor;
    private readonly RequestScopedClock _requestScopedClock;
    private readonly IServiceProvider _serviceProvider;

    public DiLifetimeLabController(
        ITransientLifetimeMarker transientFromConstructor,
        IScopedLifetimeMarker scopedFromConstructor,
        ISingletonLifetimeMarker singletonFromConstructor,
        RequestScopedClock requestScopedClock,
        IServiceProvider serviceProvider)
    {
        _transientFromConstructor = transientFromConstructor;
        _scopedFromConstructor = scopedFromConstructor;
        _singletonFromConstructor = singletonFromConstructor;
        _requestScopedClock = requestScopedClock;
        _serviceProvider = serviceProvider;
    }

    [HttpGet]
    [ProducesResponseType(typeof(DiLifetimeLabResponse), StatusCodes.Status200OK)]
    public ActionResult<DiLifetimeLabResponse> GetSnapshot()
    {
        var transientResolvedAgain = _serviceProvider.GetRequiredService<ITransientLifetimeMarker>();
        var scopedResolvedAgain = _serviceProvider.GetRequiredService<IScopedLifetimeMarker>();
        var singletonResolvedAgain = _serviceProvider.GetRequiredService<ISingletonLifetimeMarker>();

        return Ok(new DiLifetimeLabResponse(
            Request.HttpContext.TraceIdentifier,
            _requestScopedClock.RequestScopeId,
            new LifetimePair(
                _transientFromConstructor.InstanceId,
                transientResolvedAgain.InstanceId,
                _transientFromConstructor.InstanceId != transientResolvedAgain.InstanceId),
            new LifetimePair(
                _scopedFromConstructor.InstanceId,
                scopedResolvedAgain.InstanceId,
                _scopedFromConstructor.InstanceId == scopedResolvedAgain.InstanceId),
            new LifetimePair(
                _singletonFromConstructor.InstanceId,
                singletonResolvedAgain.InstanceId,
                _singletonFromConstructor.InstanceId == singletonResolvedAgain.InstanceId),
            Notes: new[]
            {
                "Transient: new instance on every resolution (constructor vs GetRequiredService).",
                "Scoped: same instance within one HTTP request scope.",
                "Singleton: same instance for the application lifetime (compare across requests).",
                "RequestScopedClock is scoped — its RequestScopeId changes on each HTTP request.",
            }));
    }
}

public sealed record LifetimePair(Guid FirstResolutionId, Guid SecondResolutionId, bool MatchesExpectedLifetimeBehavior);

public sealed record DiLifetimeLabResponse(
    string HttpRequestTraceId,
    Guid RequestScopeId,
    LifetimePair Transient,
    LifetimePair Scoped,
    LifetimePair Singleton,
    IReadOnlyList<string> Notes);
