using AIKnowledgeAssistant.Api.Contracts.Tokens;
using AIKnowledgeAssistant.Application.Chat.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIKnowledgeAssistant.Api.Controllers;

/// <summary>
/// P3-03: Compare short vs long context token usage, latency, and truncation.
/// </summary>
[ApiController]
[Route("api/labs/tokens")]
[Authorize]
public sealed class TokenLabController : ControllerBase
{
    private readonly ITokenContextLabService _tokenContextLabService;

    public TokenLabController(ITokenContextLabService tokenContextLabService) =>
        _tokenContextLabService = tokenContextLabService;

    [HttpPost("context-experiment")]
    [ProducesResponseType(typeof(TokenContextExperimentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<TokenContextExperimentResponseDto>> RunContextExperimentAsync(
        [FromBody] TokenContextExperimentRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await _tokenContextLabService.RunContextExperimentAsync(
            new TokenContextExperimentInput(request.Question, request.GroundingContext),
            cancellationToken);

        return Ok(ToDto(result));
    }

    private static TokenContextExperimentResponseDto ToDto(TokenContextExperimentResult result) =>
        new()
        {
            Question = result.Question,
            ContextWindowTokens = result.ContextWindowTokens,
            MaxPromptTokens = result.MaxPromptTokens,
            Notes = result.Notes,
            Scenarios = result.Scenarios
                .Select(scenario => new TokenContextScenarioDto
                {
                    Scenario = scenario.Scenario,
                    Description = scenario.Description,
                    TruncationApplied = scenario.TruncationApplied,
                    TruncationStrategy = scenario.TruncationStrategy,
                    ContextCharacterCount = scenario.ContextCharacterCount,
                    EstimatedInputTokensBeforeCall = scenario.EstimatedInputTokensBeforeCall,
                    PromptTokens = scenario.TokenUsage.PromptTokens,
                    CompletionTokens = scenario.TokenUsage.CompletionTokens,
                    EstimatedPromptTokens = scenario.TokenUsage.EstimatedPromptTokens,
                    EstimatedCompletionTokens = scenario.TokenUsage.EstimatedCompletionTokens,
                    DurationMs = scenario.DurationMs,
                    EstimatedInputCost = scenario.EstimatedInputCost,
                    AssistantPreview = scenario.AssistantPreview,
                })
                .ToList(),
        };
}
