using System.ComponentModel.DataAnnotations;

namespace AIKnowledgeAssistant.Api.Contracts.Tokens;

public sealed class TokenContextExperimentRequestDto
{
    [Required]
    [MinLength(1)]
    public string Question { get; set; } = string.Empty;

    public string? GroundingContext { get; set; }
}

public sealed class TokenContextExperimentResponseDto
{
    public required string Question { get; set; }

    public int ContextWindowTokens { get; set; }

    public int MaxPromptTokens { get; set; }

    public required IReadOnlyList<TokenContextScenarioDto> Scenarios { get; set; }

    public required IReadOnlyList<string> Notes { get; set; }
}

public sealed class TokenContextScenarioDto
{
    public required string Scenario { get; set; }

    public required string Description { get; set; }

    public bool TruncationApplied { get; set; }

    public required string TruncationStrategy { get; set; }

    public int ContextCharacterCount { get; set; }

    public int EstimatedInputTokensBeforeCall { get; set; }

    public int? PromptTokens { get; set; }

    public int? CompletionTokens { get; set; }

    public int EstimatedPromptTokens { get; set; }

    public int EstimatedCompletionTokens { get; set; }

    public long DurationMs { get; set; }

    public decimal EstimatedInputCost { get; set; }

    public required string AssistantPreview { get; set; }
}
