using System.Text.Json;
using AIKnowledgeAssistant.Application.Chat;
using AIKnowledgeAssistant.Application.Configuration;
using AIKnowledgeAssistant.Application.Chat.Tokens;

namespace AIKnowledgeAssistant.Infrastructure.Chat;

public static class OllamaStreamLineParser
{
    public static IEnumerable<ChatStreamChunk> ParseLine(string line, string model)
    {
        using var document = JsonDocument.Parse(line);
        var root = document.RootElement;

        if (root.TryGetProperty("message", out var messageElement) &&
            messageElement.TryGetProperty("content", out var contentElement))
        {
            var delta = contentElement.GetString();
            if (!string.IsNullOrEmpty(delta))
            {
                yield return new ChatStreamChunk(delta, IsFinal: false);
            }
        }

        if (root.TryGetProperty("done", out var doneElement) && doneElement.GetBoolean())
        {
            var promptTokens = root.TryGetProperty("prompt_eval_count", out var promptElement)
                ? promptElement.GetInt32()
                : (int?)null;
            var completionTokens = root.TryGetProperty("eval_count", out var evalElement)
                ? evalElement.GetInt32()
                : (int?)null;
            var durationMs = root.TryGetProperty("total_duration", out var durationElement)
                ? durationElement.GetInt64() / 1_000_000
                : (long?)null;

            var usage = new TokenUsage(
                promptTokens,
                completionTokens,
                promptTokens ?? 0,
                completionTokens ?? 0);

            yield return new ChatStreamChunk(
                string.Empty,
                IsFinal: true,
                Model: model,
                Provider: LlmProviders.Ollama,
                TokenUsage: usage,
                ProviderDurationMs: durationMs);
        }
    }
}
