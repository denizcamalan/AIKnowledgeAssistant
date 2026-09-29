using System.ComponentModel.DataAnnotations;

namespace AIKnowledgeAssistant.Api.Contracts.PromptLab;

public sealed class PromptLabCompareRequestDto
{
    [Required]
    [MinLength(1)]
    public string Question { get; set; } = string.Empty;

    public string? GroundingContext { get; set; }
}

public sealed class PromptLabCompareResponseDto
{
    public required string Question { get; set; }

    public required string GroundingContextUsed { get; set; }

    public required IReadOnlyList<PromptVariantRunDto> Variants { get; set; }

    public required IReadOnlyList<string> EvaluationCriteria { get; set; }
}

public sealed class PromptVariantRunDto
{
    public required string Variant { get; set; }

    public required string Intent { get; set; }

    public required string SystemMessage { get; set; }

    public required string UserMessage { get; set; }

    public int FewShotTurnCount { get; set; }

    public required string AssistantMessage { get; set; }

    public required string Model { get; set; }

    public required string Provider { get; set; }

    public long DurationMs { get; set; }

    public int ResponseCharacterCount { get; set; }
}
