# Structured output and JSON validation (P3-04)

Classify an uploaded document with a **typed JSON** response from the LLM, validate it, **retry** on invalid output, then **fallback** if retries are exhausted.

## Endpoint

`POST /api/documents/{id}/classify` (JWT required)

Response fields include `summary`, `category`, `keywords`, `confidence`, plus `usedFallback` and `attemptCount`.

## Schema

| Field | Rules |
|-------|--------|
| `summary` | 1–500 characters |
| `category` | `policy`, `technical`, or `general` |
| `keywords` | 1–8 non-empty strings |
| `confidence` | 0.0–1.0 |

## Flow

1. Load document metadata and read a text **excerpt** (first 4000 chars from stored file).
2. Call Ollama with `format: json` and a strict system schema description.
3. **Parse** model text (`StructuredLlmJsonParser`): strip markdown fences, deserialize, validate.
4. On failure, **retry** up to `Llm:StructuredOutputMaxAttempts` with the previous error in the prompt.
5. If still invalid, return a **fallback** DTO (`usedFallback: true`, `confidence: 0`).

## Why validate if the model returns JSON?

Models can emit invalid JSON, wrong types, extra prose, or schema drift. Validation keeps downstream code safe and gives a controlled retry/fallback path.

## Configuration

```json
"Llm": {
  "StructuredOutputMaxAttempts": 3
}
```

## Related

- [llm-ollama.md](llm-ollama.md)
- [token-context-lab.md](token-context-lab.md)
