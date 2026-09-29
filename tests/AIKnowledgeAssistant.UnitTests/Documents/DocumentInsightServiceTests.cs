using AIKnowledgeAssistant.Application.Chat;
using AIKnowledgeAssistant.Application.Chat.Tokens;
using AIKnowledgeAssistant.Application.Configuration;
using AIKnowledgeAssistant.Application.Documents;
using AIKnowledgeAssistant.Application.Documents.StructuredOutput;
using AIKnowledgeAssistant.Domain.Documents;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AIKnowledgeAssistant.UnitTests.Documents;

public sealed class DocumentInsightServiceTests
{
    [Fact]
    public async Task ClassifyAsync_WithInvalidThenValidJson_RetriesAndSucceeds()
    {
        var documentId = Guid.NewGuid();
        var repository = new FakeDocumentRepository
        {
            Document = new Document
            {
                Id = documentId,
                DisplayName = "Policy",
                OriginalFileName = "policy.txt",
                ContentType = "text/plain",
                StoragePath = $"{documentId:N}/policy.txt",
                Status = DocumentStatus.Uploaded,
                SizeBytes = 10,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                UpdatedAtUtc = DateTimeOffset.UtcNow,
            },
        };
        var storage = new FakeFileStorage();
        await storage.SaveAsync(documentId, new MemoryStream("Uzaktan çalışma haftada 2 gün."u8.ToArray()), "policy.txt", CancellationToken.None);

        var chat = new SequenceChatService(
        [
            "not-json",
            """{"summary":"Politika özeti","category":"policy","keywords":["remote"],"confidence":0.9}""",
        ]);

        var service = new DocumentInsightService(
            repository,
            storage,
            chat,
            new StructuredLlmJsonParser(),
            Options.Create(new LlmOptions { StructuredOutputMaxAttempts = 3 }),
            NullLogger<DocumentInsightService>.Instance);

        var result = await service.ClassifyAsync(documentId, CancellationToken.None);

        Assert.False(result.UsedFallback);
        Assert.Equal(2, result.AttemptCount);
        Assert.Equal("policy", result.Classification.Category);
    }

    [Fact]
    public async Task ClassifyAsync_WhenAllAttemptsInvalid_UsesFallback()
    {
        var documentId = Guid.NewGuid();
        var repository = new FakeDocumentRepository
        {
            Document = new Document
            {
                Id = documentId,
                DisplayName = "Notes",
                OriginalFileName = "notes.txt",
                ContentType = "text/plain",
                StoragePath = $"{documentId:N}/notes.txt",
                Status = DocumentStatus.Uploaded,
                SizeBytes = 5,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                UpdatedAtUtc = DateTimeOffset.UtcNow,
            },
        };
        var storage = new FakeFileStorage();
        await storage.SaveAsync(documentId, new MemoryStream("text"u8.ToArray()), "notes.txt", CancellationToken.None);

        var service = new DocumentInsightService(
            repository,
            storage,
            new SequenceChatService(["bad", "also bad", "still bad"]),
            new StructuredLlmJsonParser(),
            Options.Create(new LlmOptions { StructuredOutputMaxAttempts = 3 }),
            NullLogger<DocumentInsightService>.Instance);

        var result = await service.ClassifyAsync(documentId, CancellationToken.None);

        Assert.True(result.UsedFallback);
        Assert.Equal("general", result.Classification.Category);
        Assert.Equal(0, result.Classification.Confidence);
    }

    private sealed class SequenceChatService(IReadOnlyList<string> responses) : IAiChatService
    {
        private int _index;

        public Task<ChatReply> CompleteAsync(ChatPrompt prompt, CancellationToken cancellationToken)
        {
            var content = responses[Math.Min(_index++, responses.Count - 1)];
            return Task.FromResult(new ChatReply(
                content,
                "test",
                "Test",
                new TokenUsage(1, 1, 1, 1)));
        }
    }
}
