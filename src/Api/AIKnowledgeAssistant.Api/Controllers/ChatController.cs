using AIKnowledgeAssistant.Api.Contracts.Chat;
using AIKnowledgeAssistant.Api.Sse;
using AIKnowledgeAssistant.Application.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIKnowledgeAssistant.Api.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public sealed class ChatController : ControllerBase
{
    private readonly IAiChatService _chatService;
    private readonly IChatStreamService _chatStreamService;

    public ChatController(IAiChatService chatService, IChatStreamService chatStreamService)
    {
        _chatService = chatService;
        _chatStreamService = chatStreamService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ChatResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<ChatResponseDto>> CompleteAsync(
        [FromBody] ChatRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var reply = await _chatService.CompleteAsync(
            new ChatPrompt(request.Message, request.SystemMessage),
            cancellationToken);

        return Ok(new ChatResponseDto
        {
            Message = reply.Content,
            Model = reply.Model,
            Provider = reply.Provider,
            PromptTokens = reply.TokenUsage.PromptTokens,
            CompletionTokens = reply.TokenUsage.CompletionTokens,
            EstimatedPromptTokens = reply.TokenUsage.EstimatedPromptTokens,
            EstimatedCompletionTokens = reply.TokenUsage.EstimatedCompletionTokens,
            ProviderDurationMs = reply.ProviderDurationMs,
        });
    }

    [HttpPost("stream")]
    [Produces("text/event-stream")]
    public async Task StreamAsync([FromBody] ChatRequestDto request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            HttpContext.RequestAborted);

        try
        {
            await foreach (var chunk in _chatStreamService.StreamAsync(
                               new ChatPrompt(request.Message, request.SystemMessage),
                               linkedCts.Token))
            {
                if (!string.IsNullOrEmpty(chunk.TextDelta))
                {
                    await SseResponseWriter.WriteEventAsync(
                        Response,
                        "delta",
                        new { text = chunk.TextDelta },
                        linkedCts.Token);
                }

                if (chunk.IsFinal)
                {
                    await SseResponseWriter.WriteEventAsync(
                        Response,
                        "done",
                        new
                        {
                            model = chunk.Model,
                            provider = chunk.Provider,
                            promptTokens = chunk.TokenUsage?.PromptTokens,
                            completionTokens = chunk.TokenUsage?.CompletionTokens,
                            providerDurationMs = chunk.ProviderDurationMs,
                        },
                        linkedCts.Token);
                }
            }
        }
        catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested)
        {
            // Client disconnected; upstream cancellation is expected.
        }
        catch (AiChatProviderException exception)
        {
            if (!HttpContext.RequestAborted.IsCancellationRequested)
            {
                await SseResponseWriter.WriteEventAsync(
                    Response,
                    "error",
                    new { message = exception.Message },
                    CancellationToken.None);
            }
        }
    }
}
