using AIKnowledgeAssistant.Application.Configuration;
using AIKnowledgeAssistant.Application.Ingestion;
using AIKnowledgeAssistant.Domain.Documents;
using AIKnowledgeAssistant.UnitTests.Documents;
using AIKnowledgeAssistant.UnitTests.Embeddings;
using Microsoft.Extensions.Options;

namespace AIKnowledgeAssistant.UnitTests.Ingestion;

public sealed class DocumentTextIngestionProcessorTests
{
    [Fact]
    public async Task ProcessAsync_TextFile_PersistsChunksWithOffsetsAndStableIds()
    {
        var storage = new FakeFileStorage();
        var chunks = new FakeDocumentChunkRepository();
        var documentId = Guid.NewGuid();
        var content = new string('a', 25);
        var path = await storage.SaveAsync(
            documentId,
            new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)),
            "notes.txt",
            CancellationToken.None);

        var document = new Document
        {
            Id = documentId,
            OriginalFileName = "notes.txt",
            DisplayName = "notes.txt",
            ContentType = "text/plain",
            SizeBytes = content.Length,
            StoragePath = path,
            Status = DocumentStatus.Processing,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
        };

        var processor = new DocumentTextIngestionProcessor(
            storage,
            new PlainTextExtractor(),
            new DeterministicTextChunker(),
            chunks,
            new NoOpChunkEmbeddingService(),
            Options.Create(new IngestionOptions { ChunkSize = 10, ChunkOverlap = 2 }),
            TimeProvider.System);

        await processor.ProcessAsync(document, CancellationToken.None);

        var saved = chunks.GetChunks(documentId);
        Assert.Equal(3, saved.Count);
        Assert.All(saved, c => Assert.Equal(documentId, c.DocumentId));
        Assert.Equal(0, saved[0].StartOffset);
        Assert.True(saved[^1].EndOffset <= content.Length);

        await processor.ProcessAsync(document, CancellationToken.None);
        var replaced = chunks.GetChunks(documentId);
        Assert.Equal(saved[0].Id, replaced[0].Id);
    }
}
