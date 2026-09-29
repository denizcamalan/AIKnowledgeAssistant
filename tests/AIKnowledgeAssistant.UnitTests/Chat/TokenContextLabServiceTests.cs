using AIKnowledgeAssistant.Application.Chat;
using AIKnowledgeAssistant.Application.Chat.Tokens;
using AIKnowledgeAssistant.Application.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AIKnowledgeAssistant.UnitTests.Chat;

public sealed class TokenContextLabServiceTests
{
    [Fact]
    public async Task RunContextExperimentAsync_ProducesThreeScenarios()
    {
        var chat = new RecordingChatService();
        var service = new TokenContextLabService(
            chat,
            new HeadTailContextTruncationService(new HeuristicTokenEstimator()),
            new HeuristicTokenEstimator(),
            Options.Create(new LlmOptions { MaxPromptTokens = 512, CompletionTokenReserve = 64 }),
            NullLogger<TokenContextLabService>.Instance);

        var result = await service.RunContextExperimentAsync(
            new TokenContextExperimentInput("Haftada kaç gün uzaktan çalışabilirim?"),
            CancellationToken.None);

        Assert.Equal(3, chat.CallCount);
        Assert.Equal(3, result.Scenarios.Count);
        Assert.Contains(result.Scenarios, scenario => scenario.Scenario == "long-context-truncated");
        Assert.True(result.Scenarios.Single(scenario => scenario.Scenario == "long-context-truncated").TruncationApplied);
    }

    private sealed class RecordingChatService : IAiChatService
    {
        public int CallCount { get; private set; }

        public Task<ChatReply> CompleteAsync(ChatPrompt prompt, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(new ChatReply(
                "cevap",
                "model",
                "Test",
                new TokenUsage(CallCount * 200, 20, CallCount * 150, 20),
                ProviderDurationMs: CallCount * 10));
        }
    }
}
