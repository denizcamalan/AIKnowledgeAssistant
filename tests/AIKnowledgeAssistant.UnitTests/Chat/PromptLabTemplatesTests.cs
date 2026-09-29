using AIKnowledgeAssistant.Application.Chat.PromptLab;

namespace AIKnowledgeAssistant.UnitTests.Chat;

public sealed class PromptLabTemplatesTests
{
    [Fact]
    public void Baseline_UsesQuestionOnly()
    {
        var prompt = PromptLabTemplates.Baseline("  Soru?  ");

        Assert.Equal("Soru?", prompt.Message);
        Assert.Null(prompt.SystemMessage);
        Assert.Null(prompt.FewShotExamples);
    }

    [Fact]
    public void Constrained_IncludesSystemAndFewShot()
    {
        var prompt = PromptLabTemplates.Constrained("Kaç gün?");

        Assert.NotNull(prompt.SystemMessage);
        Assert.Equal(2, prompt.FewShotExamples?.Count);
    }

    [Fact]
    public void Grounded_EmbedsContextInUserMessage()
    {
        const string context = "Test bağlamı";
        var prompt = PromptLabTemplates.Grounded("Soru?", context);

        Assert.Contains(context, prompt.Message, StringComparison.Ordinal);
        Assert.NotNull(prompt.SystemMessage);
    }
}
