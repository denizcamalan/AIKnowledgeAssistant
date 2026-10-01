# Text extraction and chunking (P5-02)

Deterministic splitting of TXT/Markdown uploads into persisted chunks with source offsets (embeddings in P5-03).

## Flow

1. `POST /api/documents/{id}/ingestion/run` (P5-01 pipeline).
2. `DocumentTextIngestionProcessor` reads the file from storage, extracts UTF-8 text, chunks, replaces rows in `document_chunks`.
3. `GET /api/documents/{id}/chunks` returns ordered chunks for inspection and future RAG.

PDF upload is allowed at the API boundary but ingestion **fails** with `DocumentStatus.Failed` until a PDF extractor exists.

## Configuration (`Ingestion`)

| Setting | Default | Role |
|---------|---------|------|
| `ChunkSize` | 800 | Max characters per chunk |
| `ChunkOverlap` | 120 | Shared tail/head between consecutive chunks |
| `TextExtensions` | `.txt`, `.md` | Files processed by the text extractor |

Step size = `ChunkSize - ChunkOverlap`. Same text + settings → same slices (unit-tested).

## Chunk identity and metadata

Each row stores:

- **`Id`** — `Guid.CreateVersion5(documentId, chunkIndex)` so re-ingestion is idempotent per index
- **`ChunkIndex`** — order in the document
- **`StartOffset` / `EndOffset`** — character offsets in the extracted plain text (citation anchors)
- **`Text`** — chunk body

## Interview notes

- **Chunk size:** balance context window, retrieval precision, and cost; tune with eval set (P5-06).
- **Overlap:** reduces boundary cuts that split sentences/facts; costs more storage and embed work.
- **Dezavantaj:** fixed character windows ignore paragraph/heading structure; semantic chunking is heavier.

## Related

- [document-ingestion-pipeline.md](document-ingestion-pipeline.md)
- P5-03 embeddings — [embeddings.md](embeddings.md)
