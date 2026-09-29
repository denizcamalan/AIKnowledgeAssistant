using System.Text.Json;

namespace AIKnowledgeAssistant.Api.Sse;

public static class SseResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task WriteEventAsync(
        HttpResponse response,
        string eventType,
        object data,
        CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(data, JsonOptions);
        await response.WriteAsync($"event: {eventType}\n", cancellationToken);
        await response.WriteAsync($"data: {payload}\n\n", cancellationToken);
        await response.Body.FlushAsync(cancellationToken);
    }
}
