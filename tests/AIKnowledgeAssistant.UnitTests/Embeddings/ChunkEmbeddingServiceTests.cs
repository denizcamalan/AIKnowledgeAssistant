using AIKnowledgeAssistant.Application.Configuration;
using AIKnowledgeAssistant.Application.Embeddings;
using AIKnowledgeAssistant.Application.Ingestion;
using AIKnowledgeAssistant.Domain.Documents;
using AIKnowledgeAssistant.UnitTests.Documents;
using AIKnowledgeAssistant.UnitTests.Ingestion;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AIKnowledgeAssistant.UnitTests.Embeddings;

public sealed class ChunkEmbeddingServiceTests
{
    [Fact]
    public async Task EmbedDocumentAsync_WhenDimensionsMatch_PersistsEmbeddings()
    {
        var repository = new FakeDocumentChunkRepository();
        var documentId = Guid.NewGuid();
        var chunk = new DocumentChunk
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            ChunkIndex = 0,
            Text = "hello",
            StartOffset = 0,
            EndOffset = 5,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };
        await repository.ReplaceForDocumentAsync(documentId, [chunk], CancellationToken.None);

        var service = new ChunkEmbeddingService(
            repository,
            new FixedEmbeddingClientFactory(768),
            Options.Create(new EmbeddingOptions { ExpectedDimensions = 768, BatchSize = 8, MaxAttempts = 1 }),
            NullLogger<ChunkEmbeddingService>.Instance);

        await service.EmbedDocumentAsync(documentId, CancellationToken.None);

        var saved = repository.GetChunks(documentId).Single();
        Assert.NotNull(saved.Embedding);
        Assert.Equal(768, saved.Embedding!.Length);
        Assert.Equal("nomic-embed-text", saved.EmbeddingModel);
    }

    [Fact]
    public async Task EmbedDocumentAsync_WhenDimensionsMismatch_Throws()
    {
        var repository = new FakeDocumentChunkRepository();
        var documentId = Guid.NewGuid();
        await repository.ReplaceForDocumentAsync(
            documentId,
            [
                new DocumentChunk
                {
                    Id = Guid.NewGuid(),
                    DocumentId = documentId,
                    ChunkIndex = 0,
                    Text = "hello",
                    StartOffset = 0,
                    EndOffset = 5,
                    CreatedAtUtc = DateTimeOffset.UtcNow,
                },
            ],
            CancellationToken.None);

        var service = new ChunkEmbeddingService(
            repository,
            new FixedEmbeddingClientFactory(512),
            Options.Create(new EmbeddingOptions { ExpectedDimensions = 768, MaxAttempts = 1 }),
            NullLogger<ChunkEmbeddingService>.Instance);

        await Assert.ThrowsAsync<EmbeddingDimensionMismatchException>(() =>
            service.EmbedDocumentAsync(documentId, CancellationToken.None));
    }

    private sealed class FixedEmbeddingClientFactory(int dimensions) : IEmbeddingClientFactory
    {
        public IEmbeddingClient GetClient() => new FixedEmbeddingClient(dimensions);
    }

    private sealed class FixedEmbeddingClient(int dimensions) : IEmbeddingClient
    {
        public Task<IReadOnlyList<float[]>> EmbedAsync(IReadOnlyList<string> inputs, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<float[]>>(
                inputs.Select(_ => new float[dimensions]).ToList());
    }
}

internal sealed class NoOpChunkEmbeddingService : IChunkEmbeddingService
{
    public Task EmbedDocumentAsync(Guid documentId, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
