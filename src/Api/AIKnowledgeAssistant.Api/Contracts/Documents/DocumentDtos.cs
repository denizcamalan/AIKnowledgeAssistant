using System.ComponentModel.DataAnnotations;
using AIKnowledgeAssistant.Application.Documents;
using AIKnowledgeAssistant.Domain.Documents;

namespace AIKnowledgeAssistant.Api.Contracts.Documents;

public sealed record DocumentSummaryDto(
    Guid Id,
    string DisplayName,
    string OriginalFileName,
    DocumentStatus Status,
    long SizeBytes,
    DateTimeOffset CreatedAtUtc);

public sealed record DocumentDetailDto(
    Guid Id,
    string DisplayName,
    string OriginalFileName,
    string ContentType,
    DocumentStatus Status,
    long SizeBytes,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed class UpdateDocumentRequest
{
    [Required]
    [MaxLength(256)]
    public string DisplayName { get; set; } = string.Empty;
}

public static class DocumentDtoMapping
{
    public static DocumentSummaryDto ToDto(DocumentSummaryResult result) =>
        new(result.Id, result.DisplayName, result.OriginalFileName, result.Status, result.SizeBytes, result.CreatedAtUtc);

    public static DocumentDetailDto ToDto(DocumentDetailResult result) =>
        new(
            result.Id,
            result.DisplayName,
            result.OriginalFileName,
            result.ContentType,
            result.Status,
            result.SizeBytes,
            result.CreatedAtUtc,
            result.UpdatedAtUtc);

    public static DocumentClassificationResponseDto ToDto(DocumentClassificationResult result) =>
        new()
        {
            DocumentId = result.DocumentId,
            Summary = result.Classification.Summary,
            Category = result.Classification.Category,
            Keywords = result.Classification.Keywords.ToList(),
            Confidence = result.Classification.Confidence,
            UsedFallback = result.UsedFallback,
            AttemptCount = result.AttemptCount,
        };
}

public sealed class DocumentClassificationResponseDto
{
    public Guid DocumentId { get; init; }

    public required string Summary { get; init; }

    public required string Category { get; init; }

    public required IReadOnlyList<string> Keywords { get; init; }

    public double Confidence { get; init; }

    public bool UsedFallback { get; init; }

    public int AttemptCount { get; init; }
}
