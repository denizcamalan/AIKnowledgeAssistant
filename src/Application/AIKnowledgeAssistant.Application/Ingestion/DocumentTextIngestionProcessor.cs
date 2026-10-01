using System.Security.Cryptography;
using System.Text;
using AIKnowledgeAssistant.Application.Configuration;
using AIKnowledgeAssistant.Application.Documents;
using AIKnowledgeAssistant.Application.Embeddings;
using AIKnowledgeAssistant.Domain.Documents;
using Microsoft.Extensions.Options;

namespace AIKnowledgeAssistant.Application.Ingestion;

public sealed class DocumentTextIngestionProcessor : IDocumentIngestionProcessor
{
    private readonly IFileStorage _fileStorage;
    private readonly ITextExtractor _textExtractor;
    private readonly ITextChunker _chunker;
    private readonly IDocumentChunkRepository _chunkRepository;
    private readonly IChunkEmbeddingService _chunkEmbeddingService;
    private readonly IngestionOptions _options;
    private readonly TimeProvider _timeProvider;

    public DocumentTextIngestionProcessor(
        IFileStorage fileStorage,
        ITextExtractor textExtractor,
        ITextChunker chunker,
        IDocumentChunkRepository chunkRepository,
        IChunkEmbeddingService chunkEmbeddingService,
        IOptions<IngestionOptions> options,
        TimeProvider timeProvider)
    {
        _fileStorage = fileStorage;
        _textExtractor = textExtractor;
        _chunker = chunker;
        _chunkRepository = chunkRepository;
        _chunkEmbeddingService = chunkEmbeddingService;
        _options = options.Value;
        _timeProvider = timeProvider;
    }

    public async Task ProcessAsync(Document document, CancellationToken cancellationToken)
    {
        if (!_textExtractor.CanExtract(document.OriginalFileName))
        {
            throw new NotSupportedException(
                $"Text extraction for '{Path.GetExtension(document.OriginalFileName)}' is not implemented yet. Supported: {string.Join(", ", _options.TextExtensions)}.");
        }

        await using var stream = await _fileStorage.OpenReadAsync(document.StoragePath, cancellationToken);
        var text = await _textExtractor.ExtractAsync(stream, cancellationToken);
        var slices = _chunker.Chunk(text, _options.ChunkSize, _options.ChunkOverlap);
        var createdAt = _timeProvider.GetUtcNow();

        var chunks = slices
            .Select(slice => new DocumentChunk
            {
                Id = CreateChunkId(document.Id, slice.ChunkIndex),
                DocumentId = document.Id,
                ChunkIndex = slice.ChunkIndex,
                Text = slice.Text,
                StartOffset = slice.StartOffset,
                EndOffset = slice.EndOffset,
                CreatedAtUtc = createdAt,
            })
            .ToList();

        await _chunkRepository.ReplaceForDocumentAsync(document.Id, chunks, cancellationToken);
        await _chunkEmbeddingService.EmbedDocumentAsync(document.Id, cancellationToken);
    }

    internal static Guid CreateChunkId(Guid documentId, int chunkIndex)
    {
        var input = $"{documentId:N}:{chunkIndex.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return new Guid(hash.AsSpan(0, 16), true);
    }
}
