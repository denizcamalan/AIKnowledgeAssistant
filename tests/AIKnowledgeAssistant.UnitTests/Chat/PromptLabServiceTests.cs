using AIKnowledgeAssistant.Application.Chat;
using AIKnowledgeAssistant.Application.Chat.PromptLab;

namespace AIKnowledgeAssistant.UnitTests.Chat;

public sealed class PromptLabServiceTests
{
    [Fact]
    public async Task CompareVariantsAsync_RunsThreeVariantsAndCollectsMetrics()
    {
        var chat = new RecordingChatService();
        var service = new PromptLabService(chat);

        var result = await service.CompareVariantsAsync(
            new PromptLabInput("Haftada kaç gün uzaktan çalışabilirim?"),
            CancellationToken.None);

        Assert.Equal(3, chat.CallCount);
        Assert.Equal(3, result.Variants.Count);
        Assert.Contains(result.Variants, variant => variant.Variant == "baseline");
        Assert.Contains(result.Variants, variant => variant.Variant == "constrained");
        Assert.Contains(result.Variants, variant => variant.Variant == "grounded");
        Assert.Equal(2, result.Variants.Single(variant => variant.Variant == "constrained").FewShotTurnCount);
        Assert.True(result.EvaluationCriteria.Count >= 3);
        Assert.False(string.IsNullOrWhiteSpace(result.GroundingContextUsed));
    }

    [Fact]
    public async Task CompareVariantsAsync_WithBlankQuestion_Throws()
    {
        var service = new PromptLabService(new RecordingChatService());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CompareVariantsAsync(new PromptLabInput("  "), CancellationToken.None));
    }

    private sealed class RecordingChatService : IAiChatService
    {
        public int CallCount { get; private set; }

        public Task<ChatReply> CompleteAsync(ChatPrompt prompt, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(new ChatReply(
                $"reply-{CallCount}",
                "test-model",
                "Test"));
        }
    }
}
