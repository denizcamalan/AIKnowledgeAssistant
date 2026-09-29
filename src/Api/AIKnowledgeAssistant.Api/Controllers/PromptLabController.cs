using AIKnowledgeAssistant.Api.Contracts.PromptLab;
using AIKnowledgeAssistant.Application.Chat.PromptLab;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIKnowledgeAssistant.Api.Controllers;

/// <summary>
/// P3-02: Compare baseline, constrained, and grounded prompts for the same question.
/// </summary>
[ApiController]
[Route("api/labs/prompts")]
[Authorize]
public sealed class PromptLabController : ControllerBase
{
    private readonly IPromptLabService _promptLabService;

    public PromptLabController(IPromptLabService promptLabService) => _promptLabService = promptLabService;

    [HttpPost("compare")]
    [ProducesResponseType(typeof(PromptLabCompareResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<PromptLabCompareResponseDto>> CompareAsync(
        [FromBody] PromptLabCompareRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await _promptLabService.CompareVariantsAsync(
            new PromptLabInput(request.Question, request.GroundingContext),
            cancellationToken);

        return Ok(ToDto(result));
    }

    private static PromptLabCompareResponseDto ToDto(PromptLabComparisonResult result) =>
        new()
        {
            Question = result.Question,
            GroundingContextUsed = result.GroundingContextUsed,
            EvaluationCriteria = result.EvaluationCriteria,
            Variants = result.Variants
                .Select(variant => new PromptVariantRunDto
                {
                    Variant = variant.Variant,
                    Intent = variant.Intent,
                    SystemMessage = variant.SystemMessage,
                    UserMessage = variant.UserMessage,
                    FewShotTurnCount = variant.FewShotTurnCount,
                    AssistantMessage = variant.AssistantMessage,
                    Model = variant.Model,
                    Provider = variant.Provider,
                    DurationMs = variant.DurationMs,
                    ResponseCharacterCount = variant.ResponseCharacterCount,
                })
                .ToList(),
        };
}
