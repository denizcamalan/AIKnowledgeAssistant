namespace AIKnowledgeAssistant.Application.Embeddings;

public interface IChunkEmbeddingService
{
    Task EmbedDocumentAsync(Guid documentId, CancellationToken cancellationToken);
}
