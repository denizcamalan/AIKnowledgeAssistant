using AIKnowledgeAssistant.Application.Ingestion;
using AIKnowledgeAssistant.Domain.Documents;

namespace AIKnowledgeAssistant.Infrastructure.Ingestion;

/// <summary>
/// P5-01 placeholder — P5-02+ will extract text and persist chunks here.
/// </summary>
internal sealed class PlaceholderDocumentIngestionProcessor : IDocumentIngestionProcessor
{
    public Task ProcessAsync(Document document, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
