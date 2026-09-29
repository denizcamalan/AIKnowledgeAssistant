using AIKnowledgeAssistant.Application.Documents;
using AIKnowledgeAssistant.Domain.Documents;

namespace AIKnowledgeAssistant.Application.Ingestion;

public sealed class DocumentIngestionPipeline : IDocumentIngestionPipeline
{
    private readonly IDocumentRepository _repository;
    private readonly IDocumentIngestionProcessor _processor;
    private readonly TimeProvider _timeProvider;

    public DocumentIngestionPipeline(
        IDocumentRepository repository,
        IDocumentIngestionProcessor processor,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _processor = processor;
        _timeProvider = timeProvider;
    }

    public async Task<DocumentDetailResult> RunAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var document = await _repository.GetByIdAsync(documentId, cancellationToken);
        if (document is null)
        {
            throw new DocumentNotFoundException(documentId);
        }

        var now = _timeProvider.GetUtcNow();
        DocumentIngestionTransitions.BeginProcessing(document, now);
        await _repository.UpdateAsync(document, cancellationToken);

        try
        {
            await _processor.ProcessAsync(document, cancellationToken);
            DocumentIngestionTransitions.MarkReady(document, _timeProvider.GetUtcNow());
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            DocumentIngestionTransitions.MarkFailed(document, exception.Message, _timeProvider.GetUtcNow());
        }

        await _repository.UpdateAsync(document, cancellationToken);
        return DocumentMapping.ToDetail(document);
    }
}
