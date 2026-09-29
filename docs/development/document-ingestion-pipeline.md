# Document ingestion pipeline (P5-01)

Design for turning an uploaded file into searchable knowledge **without** embedding or chunk persistence yet (P5-02+).

## Status model

| `DocumentStatus` | Meaning |
|------------------|---------|
| `Uploaded` | File stored; ingestion not started |
| `Processing` | Pipeline running |
| `Ready` | Pipeline finished successfully (placeholder step today) |
| `Failed` | Pipeline step failed; reason stored for retry |

Status answers **“where is the document in the product?”** Metadata on `DocumentIngestionState` answers **“what happened last time?”** for retries:

- `AttemptCount`
- `FailureReason`
- `StartedAtUtc` / `CompletedAtUtc`

## Transitions

```text
Uploaded ──run──▶ Processing ──success──▶ Ready
Failed   ──run──▶ Processing ──failure──▶ Failed
```

Invalid (HTTP **409**): `Ready → Processing`, `Uploaded → Ready`, etc. Domain rules live in `DocumentIngestionTransitions`.

## API

`POST /api/documents/{id}/ingestion/run` — runs the pipeline synchronously (stub processor). Response is `DocumentDetailDto` including `ingestion` metadata.

Upload still ends in `Uploaded`; ingestion is explicit until P7 async queue.

## Extension point

`IDocumentIngestionProcessor` runs **text extract + chunk** for `.txt`/`.md` (see [text-chunking.md](text-chunking.md)). PDF fails until a dedicated extractor lands.

## Interview notes

- **Why separate state from metadata?** Status drives UI and guards; metadata supports observability and retry without overloading the enum.
- **Retry needs:** stable document id, last failure reason, attempt count, timestamps, and idempotent transitions from `Failed`.

## Related

- [architecture/overview.md](../architecture/overview.md) — Ingestion module
- P5-02 text extraction and chunking — [text-chunking.md](text-chunking.md)
