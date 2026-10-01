# Chunk embeddings (P5-03)

Turn chunk text into **dense vectors** stored in PostgreSQL (**pgvector**) for later similarity search (P5-04+).

## What is an embedding?

An embedding model maps text to a fixed-size float array (e.g. 768 dimensions). Semantically similar texts tend to have vectors that are **close** under cosine distance. RAG retrieval compares the **query embedding** to **chunk embeddings** with the **same model**.

## Architecture

| Piece | Role |
|-------|------|
| `IEmbeddingClient` | Provider call (batch of strings → vectors) |
| `IEmbeddingClientFactory` | Select Ollama (same pattern as chat factory) |
| `ChunkEmbeddingService` | Batch + retry + dimension check + persist |
| `DocumentTextIngestionProcessor` | After chunks saved → `EmbedDocumentAsync` |

Config section **`Embeddings`**: `Model`, `ExpectedDimensions`, `BatchSize`, `MaxAttempts`.

## Ollama

Uses `POST /api/embed` with `nomic-embed-text` by default (`ollama pull nomic-embed-text`). Base URL shares `Llm:Ollama:BaseUrl`.

## Dimension guard

If the provider returns a vector length ≠ `ExpectedDimensions`, ingestion throws **`EmbeddingDimensionMismatchException`** (502). This catches wrong model pulls or config drift before bad data hits pgvector.

## Storage

`document_chunks.embedding` → `vector(768)` + `embedding_model`. Full vectors are **not** exposed in the public chunk API (`hasEmbedding` flag only).

## Local verify

```bash
ollama pull nomic-embed-text
# upload txt → POST .../ingestion/run → GET .../chunks  (hasEmbedding: true)
```

## Interview notes

- **Same model for query and document** — otherwise distances are meaningless.
- **Batching** — fewer HTTP round-trips; watch payload size.
- **Dezavantaj:** sync embed inside HTTP ingestion; large docs block until all batches finish (queue in P7).

## Related

- [text-chunking.md](text-chunking.md)
- [ADR-0003](../adr/0003-postgresql-pgvector.md)
