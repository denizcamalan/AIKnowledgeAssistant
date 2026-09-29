namespace AIKnowledgeAssistant.Application.Ingestion;

public interface ITextChunker
{
    IReadOnlyList<TextChunk> Chunk(string text, int chunkSize, int chunkOverlap);
}

public sealed class DeterministicTextChunker : ITextChunker
{
    public IReadOnlyList<TextChunk> Chunk(string text, int chunkSize, int chunkOverlap)
    {
        if (chunkSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(chunkSize), "Chunk size must be positive.");
        }

        if (chunkOverlap < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(chunkOverlap), "Chunk overlap cannot be negative.");
        }

        if (chunkOverlap >= chunkSize)
        {
            throw new ArgumentOutOfRangeException(nameof(chunkOverlap), "Chunk overlap must be smaller than chunk size.");
        }

        if (string.IsNullOrEmpty(text))
        {
            return [];
        }

        var chunks = new List<TextChunk>();
        var step = chunkSize - chunkOverlap;
        var start = 0;
        var index = 0;

        while (start < text.Length)
        {
            var end = Math.Min(start + chunkSize, text.Length);
            chunks.Add(new TextChunk(index, text[start..end], start, end));

            if (end >= text.Length)
            {
                break;
            }

            start += step;
            index++;
        }

        return chunks;
    }
}
