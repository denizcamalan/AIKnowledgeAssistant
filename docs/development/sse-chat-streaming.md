# SSE chat streaming (P4-01 / P4-02)

Stream LLM answers to the browser with **Server-Sent Events** instead of waiting for one JSON body.

## Endpoint

`POST /api/chat/stream` (JWT, `Content-Type: application/json` body same as `/api/chat`)

Response: `text/event-stream`

## Event format

| Event | Payload | Meaning |
|-------|---------|---------|
| `started` | `{ "streamId": "..." }` | Server registered this stream; use id for stop |
| `delta` | `{ "text": "..." }` | Next token/chunk of assistant text |
| `done` | `{ "streamId", "model", "provider", ... }` | Stream finished normally |
| `stopped` | `{ "streamId", "reason": "stop-request" }` | Cancelled via `POST /api/chat/stream/stop` |
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

Two paths share one linked `CancellationToken`:

1. **Client disconnect** — `HttpContext.RequestAborted`; no `stopped` event (connection is gone).
2. **Explicit stop** — `POST /api/chat/stream/stop` with `{ "streamId": "..." }` (same JWT user as the stream). Returns `204` or `404`. Emits `stopped` on the open SSE response.

The React chat UI uses **fetch + ReadableStream** (not `EventSource`) so it can send JWT on `POST` and call stop while the stream is open. See `client/web/src/api/chatStream.ts` and `ChatScreen.tsx`.

## Stop curl

```bash
curl -X POST http://localhost:5149/api/chat/stream/stop \
  -H "Authorization: Bearer $TOKEN" \
  -H 'Content-Type: application/json' \
  -d '{"streamId":"YOUR-STREAM-ID"}'
```

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
- P4-02 React chat screen — streaming transcript + Stop button
