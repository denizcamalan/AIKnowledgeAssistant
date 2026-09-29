# LLM integration (P3-01)

The API talks to a local or remote LLM through **`IAiChatService`**. Provider-specific HTTP lives behind **`IAiChatCompletionClient`**, selected at runtime by **`IAiChatClientFactory`** from `Llm:Provider`.

## Default: Ollama + Qwen3 4B

1. Install [Ollama](https://ollama.com/).
2. Pull the model:

   ```bash
   ollama pull qwen3:4b
   ```

3. Ensure Ollama is running (default base URL `http://localhost:11434`).
4. Start the API and call chat (after login):

   ```bash
   TOKEN=$(curl -s -X POST http://localhost:5149/api/auth/login \
     -H 'Content-Type: application/json' \
     -d '{"email":"user@demo.local","password":"User123!"}' | jq -r .accessToken)

   curl -s -X POST http://localhost:5149/api/chat \
     -H "Authorization: Bearer $TOKEN" \
     -H 'Content-Type: application/json' \
     -d '{"message":"Merhaba, kısaca kendini tanıt."}'
   ```

Configuration (`appsettings.json`):

```json
"Llm": {
  "Provider": "Ollama",
  "Ollama": {
    "BaseUrl": "http://localhost:11434",
    "Model": "qwen3:4b"
  }
}
```

Override with environment variables, e.g. `Llm__Ollama__Model=llama3.2`.

No API key is required for local Ollama. Future cloud providers should use user secrets or environment variables only—never commit keys.

## Factory pattern

| Type | Role |
|------|------|
| `IAiChatService` | Application use case (`ChatPrompt` → `ChatReply`) |
| `IAiChatClientFactory` | Returns the client for the configured provider |
| `IAiChatCompletionClient` | Provider adapter (Ollama today; OpenAI/Azure later) |

Adding a provider: implement `IAiChatCompletionClient`, register it in `AddChatInfrastructure`, extend `AiChatClientFactory.GetClient()`, and document config.

## Errors

When Ollama is unreachable or returns an error, the API responds with **502 Bad Gateway** and a ProblemDetails `detail` message (includes `traceId`).
