using System.Text;
using AIKnowledgeAssistant.Application.Chat;
using AIKnowledgeAssistant.Application.Configuration;
using AIKnowledgeAssistant.Application.Documents.StructuredOutput;
using AIKnowledgeAssistant.Domain.Documents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIKnowledgeAssistant.Application.Documents;

public sealed class DocumentInsightService : IDocumentInsightService
{
    private const int MaxExcerptCharacters = 4000;

    private readonly IDocumentRepository _repository;
    private readonly IFileStorage _fileStorage;
    private readonly IAiChatService _chatService;
    private readonly IStructuredLlmJsonParser _parser;
    private readonly LlmOptions _options;
    private readonly ILogger<DocumentInsightService> _logger;

    public DocumentInsightService(
        IDocumentRepository repository,
        IFileStorage fileStorage,
        IAiChatService chatService,
        IStructuredLlmJsonParser parser,
        IOptions<LlmOptions> options,
        ILogger<DocumentInsightService> logger)
    {
        _repository = repository;
        _fileStorage = fileStorage;
        _chatService = chatService;
        _parser = parser;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<DocumentClassificationResult> ClassifyAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var document = await _repository.GetByIdAsync(documentId, cancellationToken);
        if (document is null)
        {
            throw new DocumentNotFoundException(documentId);
        }

        var excerpt = await ReadExcerptAsync(document, cancellationToken);
        var maxAttempts = Math.Max(1, _options.StructuredOutputMaxAttempts);
        string? lastRaw = null;
        string? lastError = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var prompt = BuildPrompt(document, excerpt, lastRaw, lastError, attempt);
            var reply = await _chatService.CompleteAsync(prompt, cancellationToken);
            lastRaw = reply.Content;

            try
            {
                var parsed = _parser.ParseClassification(reply.Content);
                _logger.LogInformation(
                    "Structured classification succeeded DocumentId={DocumentId} Attempt={Attempt} Category={Category}",
                    documentId,
                    attempt,
                    parsed.Value.Category);

                return new DocumentClassificationResult(
                    documentId,
                    parsed.Value,
                    UsedFallback: false,
                    attempt,
                    lastRaw);
            }
            catch (StructuredOutputParseException exception)
            {
                lastError = exception.Message;
                _logger.LogWarning(
                    exception,
                    "Structured classification parse failed DocumentId={DocumentId} Attempt={Attempt}",
                    documentId,
                    attempt);
            }
        }

        _logger.LogWarning(
            "Structured classification exhausted retries DocumentId={DocumentId}; using fallback",
            documentId);

        return new DocumentClassificationResult(
            documentId,
            BuildFallback(document),
            UsedFallback: true,
            maxAttempts,
            lastRaw);
    }

    private static ChatPrompt BuildPrompt(
        Document document,
        string excerpt,
        string? previousOutput,
        string? previousError,
        int attempt)
    {
        var systemMessage =
            """
            You classify uploaded knowledge-base documents.
            Respond with JSON only (no markdown) matching this schema:
            {
              "summary": "string, max 500 chars",
              "category": "policy|technical|general",
              "keywords": ["1-8 strings"],
              "confidence": 0.0-1.0
            }
            """;

        var builder = new StringBuilder();
        builder.AppendLine($"Display name: {document.DisplayName}");
        builder.AppendLine($"Original file: {document.OriginalFileName}");
        builder.AppendLine($"Content type: {document.ContentType}");
        builder.AppendLine("Excerpt:");
        builder.AppendLine(excerpt);

        if (attempt > 1)
        {
            builder.AppendLine();
            builder.AppendLine("Previous invalid response:");
            builder.AppendLine(previousOutput ?? "(empty)");
            builder.AppendLine($"Validation error: {previousError}");
            builder.AppendLine("Fix the JSON and return only valid JSON.");
        }

        return new ChatPrompt(
            builder.ToString(),
            systemMessage,
            RequestJsonFormat: true);
    }

    private async Task<string> ReadExcerptAsync(Document document, CancellationToken cancellationToken)
    {
        await using var stream = await _fileStorage.OpenReadAsync(document.StoragePath, cancellationToken);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: false);
        var text = await reader.ReadToEndAsync(cancellationToken);
        if (text.Length <= MaxExcerptCharacters)
        {
            return text;
        }

        return text[..MaxExcerptCharacters];
    }

    private static DocumentClassificationPayload BuildFallback(Document document)
    {
        var extension = Path.GetExtension(document.OriginalFileName).Trim('.');
        var keyword = string.IsNullOrWhiteSpace(extension) ? "document" : extension;
        return new DocumentClassificationPayload
        {
            Summary = $"Could not parse a structured model response for '{document.DisplayName}'.",
            Category = "general",
            Keywords = [keyword],
            Confidence = 0,
        };
    }
}
