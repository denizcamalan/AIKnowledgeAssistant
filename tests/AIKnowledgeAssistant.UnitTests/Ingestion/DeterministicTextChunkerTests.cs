using AIKnowledgeAssistant.Application.Ingestion;

namespace AIKnowledgeAssistant.UnitTests.Ingestion;

public sealed class DeterministicTextChunkerTests
{
    private readonly DeterministicTextChunker _chunker = new();

    [Fact]
    public void Chunk_SameInput_ProducesIdenticalSlices()
    {
        const string text = "abcdefghijklmnopqrstuvwxyz0123456789";
        var first = _chunker.Chunk(text, chunkSize: 10, chunkOverlap: 2);
        var second = _chunker.Chunk(text, chunkSize: 10, chunkOverlap: 2);

        Assert.Equal(first.Count, second.Count);
        for (var i = 0; i < first.Count; i++)
        {
            Assert.Equal(first[i].Text, second[i].Text);
            Assert.Equal(first[i].StartOffset, second[i].StartOffset);
            Assert.Equal(first[i].EndOffset, second[i].EndOffset);
            Assert.Equal(first[i].ChunkIndex, second[i].ChunkIndex);
        }
    }

    [Fact]
    public void Chunk_WithOverlap_RepeatsBoundaryCharacters()
    {
        const string text = "0123456789";
        var chunks = _chunker.Chunk(text, chunkSize: 5, chunkOverlap: 2);

        Assert.Equal(3, chunks.Count);
        Assert.Equal("01234", chunks[0].Text);
        Assert.Equal("34567", chunks[1].Text);
        Assert.Equal("6789", chunks[2].Text);
        Assert.Equal(0, chunks[0].StartOffset);
        Assert.Equal(6, chunks[2].StartOffset);
        Assert.Equal(10, chunks[2].EndOffset);
    }

    [Fact]
    public void Chunk_EmptyText_ReturnsNoChunks()
    {
        Assert.Empty(_chunker.Chunk(string.Empty, 100, 10));
    }
}
