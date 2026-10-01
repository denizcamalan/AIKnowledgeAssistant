using AIKnowledgeAssistant.Application.Configuration;
using AIKnowledgeAssistant.Application.Embeddings;
using Microsoft.Extensions.Options;

namespace AIKnowledgeAssistant.IntegrationTests.Stubs;

internal sealed class StubEmbeddingClientFactory : IEmbeddingClientFactory
{
    private readonly IEmbeddingClient _client;

    public StubEmbeddingClientFactory(IOptions<EmbeddingOptions> options)
    {
        _client = new StubEmbeddingClient(options.Value.ExpectedDimensions);
    }

    public IEmbeddingClient GetClient() => _client;

    private sealed class StubEmbeddingClient(int dimensions) : IEmbeddingClient
    {
        public Task<IReadOnlyList<float[]>> EmbedAsync(IReadOnlyList<string> inputs, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<float[]>>(
                inputs.Select(_ => CreateDeterministicVector(dimensions)).ToList());

        private static float[] CreateDeterministicVector(int dimensions)
        {
            var vector = new float[dimensions];
            for (var i = 0; i < dimensions; i++)
            {
                vector[i] = (i + 1) * 0.001f;
            }

            return vector;
        }
    }
}
