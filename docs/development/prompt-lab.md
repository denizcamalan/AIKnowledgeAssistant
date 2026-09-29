# Prompt engineering lab (P3-02)

Compare **three prompt variants** for the same user question and record metrics. Endpoint (JWT required):

`POST /api/labs/prompts/compare`

## Variants

| Variant | Intent |
|---------|--------|
| **baseline** | User question only — no system prompt or examples |
| **constrained** | System rules (Turkish, ≤3 sentences, no guessing) + one **few-shot** pair |
| **grounded** | Policy/context block in the user message + “answer only from context” system rules |

Templates live in `PromptLabTemplates` (`Application/Chat/PromptLab`).

Optional body field `groundingContext` overrides the default sample policy text used for the grounded variant.

## Evaluation criteria (compare outputs manually or in notes)

| Criterion | What to look for |
|-----------|------------------|
| `responseCharacterCount` | Constrained should usually be shorter than baseline |
| `durationMs` | Latency per variant (depends on Ollama load) |
| `grounding` | Grounded answer cites policy; refuses when context lacks the fact |
| `constraintAdherence` | Constrained: Turkish, brief, no rambling |
| `hallucinationRisk` | Baseline may invent details not in policy |

The API returns these criterion names in `evaluationCriteria` plus per-variant `assistantMessage`, timings, and character counts.

## Example request

```bash
TOKEN=$(curl -s -X POST http://localhost:5149/api/auth/login \
  -H 'Content-Type: application/json' \
  -d '{"email":"user@demo.local","password":"User123!"}' | jq -r .accessToken)

curl -s -X POST http://localhost:5149/api/labs/prompts/compare \
  -H "Authorization: Bearer $TOKEN" \
  -H 'Content-Type: application/json' \
  -d '{"question":"Haftada kaç gün uzaktan çalışabilirim?"}' | jq
```

Requires Ollama (same as P3-01). One compare call runs **three** LLM completions sequentially.

## Recorded experiment

A sample JSON snapshot (stubbed run shape) is kept at [docs/experiments/p3-02-prompt-comparison-record.json](../experiments/p3-02-prompt-comparison-record.json). After a real Ollama run, paste your output there or in Notion learning notes.

## Related

- P3-01 LLM wiring: [llm-ollama.md](llm-ollama.md)
- RAG grounding in production will reuse the grounded pattern with retrieved chunks (P5+)
