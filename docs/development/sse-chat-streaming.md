# SSE chat streaming (P4-01)

Stream LLM answers to the browser with **Server-Sent Events** instead of waiting for one JSON body.

## Endpoint

`POST /api/chat/stream` (JWT, `Content-Type: application/json` body same as `/api/chat`)

Response: `text/event-stream`

## Event format

| Event | Payload | Meaning |
|-------|---------|---------|
| `delta` | `{ "text": "..." }` | Next token/chunk of assistant text |
| `done` | `{ "model", "provider", "promptTokens", "completionTokens", "providerDurationMs" }` | Stream finished |
| `error` | `{ "message": "..." }` | Provider failure (before connection closes) |

Example:

```text
event: delta
data: {"text":"Mer"}

event: delta
data: {"text":"haba"}

event: done
data: {"model":"qwen3:4b","provider":"Ollama","promptTokens":12,"completionTokens":3,"providerDurationMs":842}
```

## Cancellation

The action links `HttpContext.RequestAborted` with the request token. When the client disconnects, the Ollama NDJSON read loop stops via `CancellationToken`.

## curl (requires `-N` to disable buffering)

```bash
TOKEN=...
curl -N -X POST http://localhost:5149/api/chat/stream \
  -H "Authorization: Bearer $TOKEN" \
  -H 'Content-Type: application/json' \
  -d '{"message":"Merhaba"}'
```

Requires Ollama with `stream: true` support on `/api/chat`.

## SSE vs WebSocket (interview note)

- **SSE:** one-way server → client over HTTP; simple for token streaming; auto reconnect in browsers.
- **WebSocket:** full duplex; better for collaborative editing or binary frames.

Chat completion streaming is a common SSE fit.

## Related

- [llm-ollama.md](llm-ollama.md) — non-streaming `/api/chat`
- React client wiring — follow-up task (P4 UI)
