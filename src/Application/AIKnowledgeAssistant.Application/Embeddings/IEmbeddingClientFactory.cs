namespace AIKnowledgeAssistant.Application.Embeddings;

public interface IEmbeddingClientFactory
{
    IEmbeddingClient GetClient();
}
