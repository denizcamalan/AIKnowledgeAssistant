using System.Diagnostics;

namespace AIKnowledgeAssistant.Application.Chat.PromptLab;

public sealed class PromptLabService : IPromptLabService
{
    private static readonly IReadOnlyList<string> EvaluationCriteria =
    [
        "responseCharacterCount — kısıtlı prompt daha kısa mı?",
        "durationMs — gecikme karşılaştırması (model yüküne bağlı)",
        "grounding — grounded varyant bağlam dışı bilgi uyduruyor mu?",
        "constraintAdherence — constrained Türkçe ve ≤3 cümle mi?",
        "hallucinationRisk — baseline bağlam olmadan uydurma eğilimi",
    ];

    private readonly IAiChatService _chatService;

    public PromptLabService(IAiChatService chatService) => _chatService = chatService;

    public async Task<PromptLabComparisonResult> CompareVariantsAsync(
        PromptLabInput input,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input.Question))
        {
            throw new ArgumentException("Question is required.", nameof(input));
        }

        var groundingContext = string.IsNullOrWhiteSpace(input.GroundingContext)
            ? PromptLabTemplates.DefaultGroundingContext
            : input.GroundingContext.Trim();

        var variantDefinitions = new (string Variant, string Intent, ChatPrompt Prompt)[]
        {
            ("baseline", "Yalnızca kullanıcı sorusu; system/few-shot yok.", PromptLabTemplates.Baseline(input.Question)),
            ("constrained", "System kuralları + tek few-shot örneği.", PromptLabTemplates.Constrained(input.Question)),
            ("grounded", "Politika bağlamı + yalnızca bağlama dayan talimatı.", PromptLabTemplates.Grounded(input.Question, groundingContext)),
        };

        var runs = new List<PromptVariantRunResult>(variantDefinitions.Length);
        foreach (var (variant, intent, prompt) in variantDefinitions)
        {
            var stopwatch = Stopwatch.StartNew();
            var reply = await _chatService.CompleteAsync(prompt, cancellationToken);
            stopwatch.Stop();

            runs.Add(new PromptVariantRunResult(
                Variant: variant,
                Intent: intent,
                SystemMessage: prompt.SystemMessage ?? string.Empty,
                UserMessage: prompt.Message,
                FewShotTurnCount: prompt.FewShotExamples?.Count ?? 0,
                AssistantMessage: reply.Content,
                Model: reply.Model,
                Provider: reply.Provider,
                DurationMs: stopwatch.ElapsedMilliseconds,
                ResponseCharacterCount: reply.Content.Length));
        }

        return new PromptLabComparisonResult(
            input.Question.Trim(),
            groundingContext,
            runs,
            EvaluationCriteria);
    }
}
