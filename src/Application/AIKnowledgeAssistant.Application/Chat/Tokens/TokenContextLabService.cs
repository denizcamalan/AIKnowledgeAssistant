using System.Diagnostics;
using System.Text;
using AIKnowledgeAssistant.Application.Chat.PromptLab;
using AIKnowledgeAssistant.Application.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIKnowledgeAssistant.Application.Chat.Tokens;

public sealed class TokenContextLabService : ITokenContextLabService
{
    private readonly IAiChatService _chatService;
    private readonly IContextTruncationService _truncation;
    private readonly ITokenEstimator _estimator;
    private readonly LlmOptions _options;
    private readonly ILogger<TokenContextLabService> _logger;

    public TokenContextLabService(
        IAiChatService chatService,
        IContextTruncationService truncation,
        ITokenEstimator estimator,
        IOptions<LlmOptions> options,
        ILogger<TokenContextLabService> logger)
    {
        _chatService = chatService;
        _truncation = truncation;
        _estimator = estimator;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<TokenContextExperimentResult> RunContextExperimentAsync(
        TokenContextExperimentInput input,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input.Question))
        {
            throw new ArgumentException("Question is required.", nameof(input));
        }

        var shortContext = string.IsNullOrWhiteSpace(input.GroundingContext)
            ? PromptLabTemplates.DefaultGroundingContext
            : input.GroundingContext.Trim();

        var longContext = BuildExpandedContext(shortContext);
        var contextBudget = Math.Max(256, _options.MaxPromptTokens - _options.CompletionTokenReserve);

        var truncation = _truncation.TruncateToTokenBudget(longContext, contextBudget);

        var scenarios = new (string Scenario, string Description, string Context, bool Truncated, string Strategy)[]
        {
            ("short-context", "Kısa bağlam — truncation yok.", shortContext, false, string.Empty),
            ("long-context-raw", "Uzun bağlam — truncation uygulanmadan gönderilir.", longContext, false, string.Empty),
            (
                "long-context-truncated",
                "Uzun bağlam — head/tail truncation ile token bütçesine sığdırılır.",
                truncation.Text,
                truncation.WasTruncated,
                truncation.Strategy),
        };

        var results = new List<TokenContextScenarioResult>(scenarios.Length);
        foreach (var (scenario, description, context, truncatedFlag, strategy) in scenarios)
        {
            var prompt = PromptLabTemplates.Grounded(input.Question.Trim(), context);
            var estimatedInput = EstimatePromptTokens(prompt);

            var stopwatch = Stopwatch.StartNew();
            var reply = await _chatService.CompleteAsync(prompt, cancellationToken);
            stopwatch.Stop();

            var usage = reply.TokenUsage;
            _logger.LogInformation(
                "Token lab scenario={Scenario} PromptTokens={PromptTokens} CompletionTokens={CompletionTokens} EstimatedPrompt={EstimatedPrompt} DurationMs={DurationMs}",
                scenario,
                usage.PromptTokens,
                usage.CompletionTokens,
                usage.EstimatedPromptTokens,
                stopwatch.ElapsedMilliseconds);

            results.Add(new TokenContextScenarioResult(
                scenario,
                description,
                truncatedFlag,
                strategy,
                context.Length,
                estimatedInput,
                usage,
                stopwatch.ElapsedMilliseconds,
                EstimateInputCost(usage),
                Preview(reply.Content)));
        }

        return new TokenContextExperimentResult(
            input.Question.Trim(),
            _options.ContextWindowTokens,
            _options.MaxPromptTokens,
            results,
            Notes);
    }

    private int EstimatePromptTokens(ChatPrompt prompt)
    {
        var builder = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(prompt.SystemMessage))
        {
            builder.AppendLine(prompt.SystemMessage);
        }

        if (prompt.FewShotExamples is not null)
        {
            foreach (var example in prompt.FewShotExamples)
            {
                builder.AppendLine(example.Content);
            }
        }

        builder.AppendLine(prompt.Message);
        return _estimator.Estimate(builder.ToString());
    }

    private decimal EstimateInputCost(TokenUsage usage)
    {
        var promptTokens = usage.PromptTokens ?? usage.EstimatedPromptTokens;
        return Math.Round(promptTokens * _options.EstimatedCostPer1KInputTokens / 1000m, 6);
    }

    private static string BuildExpandedContext(string seed)
    {
        var builder = new StringBuilder(seed);
        for (var i = 0; i < 60; i++)
        {
            builder.AppendLine();
            builder.Append("Ek politika maddesi #").Append(i + 1).Append(": ");
            builder.Append("Bu paragraf token/context deneyi için yapay olarak uzatılmış metindir. ");
            builder.Append("Gerçek RAG'ta benzer uzunluk çok sayıda chunk birleştirildiğinde oluşur.");
        }

        return builder.ToString();
    }

    private static string Preview(string content) =>
        content.Length <= 160 ? content : content[..160] + "…";

    private static readonly IReadOnlyList<string> Notes =
    [
        "promptTokens/completionTokens Ollama'dan gelir; yoksa Estimated* heuristic kullanılır.",
        "Uzun context genelde prompt_eval_count ve durationMs artırır.",
        "Truncation RAG'ta retrieval sonrası bütçe aşımını önlemek için kullanılır.",
        "EstimatedInputCost yerel Ollama için 0; bulut fiyatlandırmasını simüle etmek için config değiştirilebilir.",
    ];
}
