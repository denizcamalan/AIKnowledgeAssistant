using System.ComponentModel.DataAnnotations;

namespace AIKnowledgeAssistant.Api.Contracts.Chat;

public sealed class ChatRequestDto
{
    [Required]
    [MinLength(1)]
    public string Message { get; set; } = string.Empty;

    public string? SystemMessage { get; set; }
}

public sealed class ChatResponseDto
{
    public required string Message { get; set; }

    public required string Model { get; set; }

    public required string Provider { get; set; }
}
