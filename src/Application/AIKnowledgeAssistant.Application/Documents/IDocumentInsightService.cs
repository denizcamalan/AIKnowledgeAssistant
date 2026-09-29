using AIKnowledgeAssistant.Application.Documents.StructuredOutput;

namespace AIKnowledgeAssistant.Application.Documents;

public interface IDocumentInsightService
{
    Task<DocumentClassificationResult> ClassifyAsync(Guid documentId, CancellationToken cancellationToken);
}

public sealed record DocumentClassificationResult(
    Guid DocumentId,
    DocumentClassificationPayload Classification,
    bool UsedFallback,
    int AttemptCount,
    string? LastRawModelOutput);
