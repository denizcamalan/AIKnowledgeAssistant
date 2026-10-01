using AIKnowledgeAssistant.Application.Configuration;
using AIKnowledgeAssistant.Application.Ingestion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIKnowledgeAssistant.Application.Embeddings;

public sealed class ChunkEmbeddingService : IChunkEmbeddingService
{
    private readonly IDocumentChunkRepository _chunks;
    private readonly IEmbeddingClientFactory _clientFactory;
    private readonly EmbeddingOptions _options;
    private readonly ILogger<ChunkEmbeddingService> _logger;

    public ChunkEmbeddingService(
        IDocumentChunkRepository chunks,
        IEmbeddingClientFactory clientFactory,
        IOptions<EmbeddingOptions> options,
        ILogger<ChunkEmbeddingService> logger)
    {
        _chunks = chunks;
        _clientFactory = clientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task EmbedDocumentAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var chunkList = await _chunks.ListByDocumentIdAsync(documentId, cancellationToken);
        if (chunkList.Count == 0)
        {
            return;
        }

        var client = _clientFactory.GetClient();
        var model = _options.Model;

        for (var offset = 0; offset < chunkList.Count; offset += _options.BatchSize)
        {
            var batch = chunkList.Skip(offset).Take(_options.BatchSize).ToList();
            var texts = batch.Select(c => c.Text).ToList();
            var vectors = await EmbedWithRetryAsync(client, texts, cancellationToken);

            for (var i = 0; i < batch.Count; i++)
            {
                ValidateDimensions(vectors[i]);
                batch[i].Embedding = vectors[i];
                batch[i].EmbeddingModel = model;
            }

            await _chunks.UpdateEmbeddingsAsync(batch, cancellationToken);
        }
    }

    private async Task<IReadOnlyList<float[]>> EmbedWithRetryAsync(
        IEmbeddingClient client,
        IReadOnlyList<string> inputs,
        CancellationToken cancellationToken)
    {
        Exception? lastException = null;
        for (var attempt = 1; attempt <= _options.MaxAttempts; attempt++)
        {
            try
            {
                return await client.EmbedAsync(inputs, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception) when (exception is AiEmbeddingProviderException)
            {
                lastException = exception;
                _logger.LogWarning(
                    exception,
                    "Embedding batch failed (attempt {Attempt}/{MaxAttempts}).",
                    attempt,
                    _options.MaxAttempts);

                if (attempt >= _options.MaxAttempts)
                {
                    break;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), cancellationToken);
            }
        }

        throw new AiEmbeddingProviderException(
            "Embedding provider failed after retries.",
            lastException ?? new InvalidOperationException("Unknown embedding failure."));
    }

    private void ValidateDimensions(float[] vector)
    {
        if (vector.Length != _options.ExpectedDimensions)
        {
            throw new EmbeddingDimensionMismatchException(_options.ExpectedDimensions, vector.Length);
        }
    }
}
