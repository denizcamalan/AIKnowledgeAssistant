using AIKnowledgeAssistant.Api.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AIKnowledgeAssistant.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class InfoController : ControllerBase
{
    private readonly ApiSettings _settings;
    private readonly IWebHostEnvironment _environment;

    public InfoController(IOptions<ApiSettings> options, IWebHostEnvironment environment)
    {
        _settings = options.Value;
        _environment = environment;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiInfoResponse), StatusCodes.Status200OK)]
    public IActionResult Get() =>
        Ok(new ApiInfoResponse(_settings.DisplayName, _environment.EnvironmentName));
}

public sealed record ApiInfoResponse(string DisplayName, string EnvironmentName);
