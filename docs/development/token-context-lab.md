# Token and context window lab (P3-03)

Measure how **prompt size** affects tokens, latency, and (optionally) cost. Every chat completion is **logged** with token fields.

## Concepts

| Term | Meaning |
|------|---------|
| **Token** | Model input/output birimleri (~4 karakter heuristic ile tahmin edilir; Ollama `prompt_eval_count` / `eval_count` gerçek sayımı döner). |
| **Context window** | Modele tek seferde sığan maksimum token (config: `Llm:ContextWindowTokens`). |
| **Truncation** | Bağlam bütçeyi aşınca metni kısaltma — lab’da `head-tail-60-35` stratejisi. |

## Logging

`ChatService` logs (Information):

`PromptTokens`, `CompletionTokens`, `EstimatedPrompt`, `ProviderDurationMs`

`POST /api/chat` response includes the same token fields.

## Experiment endpoint

`POST /api/labs/tokens/context-experiment` (JWT)

Runs **three** grounded scenarios for the same question:

1. **short-context** — kısa politika metni  
2. **long-context-raw** — yapay olarak uzatılmış bağlam (truncation yok)  
3. **long-context-truncated** — uzun bağlam token bütçesine kısaltılmış  

Compare `promptTokens`, `durationMs`, `contextCharacterCount`, `estimatedInputCost`.

```bash
TOKEN=$(curl -s -X POST http://localhost:5149/api/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"email":"user@demo.local","password":"User123!"}' | jq -r .accessToken)

curl -s -X POST http://localhost:5149/api/labs/tokens/context-experiment \
  -H "Authorization: Bearer $TOKEN" \
  -H 'Content-Type: application/json' \
  -d '{"question":"Haftada kaç gün uzaktan çalışabilirim?"}' | jq
```

Requires Ollama. **Three LLM calls** per request.

## Configuration

```json
"Llm": {
  "ContextWindowTokens": 8192,
  "MaxPromptTokens": 2048,
  "CompletionTokenReserve": 512,
  "EstimatedCostPer1KInputTokens": 0
}
```

Set `EstimatedCostPer1KInputTokens` to simulate cloud pricing (USD per 1K input tokens).

## Record

Sample: [docs/experiments/p3-03-token-context-record.json](../experiments/p3-03-token-context-record.json)

## Related

- [prompt-lab.md](prompt-lab.md) (P3-02)
- [llm-ollama.md](llm-ollama.md) (P3-01)
