namespace AIKnowledgeAssistant.Application.Ingestion;

public sealed record TextChunk(int ChunkIndex, string Text, int StartOffset, int EndOffset);
