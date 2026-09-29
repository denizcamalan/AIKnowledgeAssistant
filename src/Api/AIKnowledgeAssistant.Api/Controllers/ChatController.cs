using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
    private readonly IChatStreamSessionRegistry _streamSessions;

    public ChatController(
        IAiChatService chatService,
        IChatStreamService chatStreamService,
        IChatStreamSessionRegistry streamSessions)
    {
        _chatService = chatService;
        _chatStreamService = chatStreamService;
        _streamSessions = streamSessions;
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

        var userId = GetCurrentUserId();
        if (userId is null)
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";

        using var session = _streamSessions.StartSession(userId);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            HttpContext.RequestAborted,
            session.Token);

        try
        {
            await SseResponseWriter.WriteEventAsync(
                Response,
                "started",
                new { streamId = session.StreamId },
                linkedCts.Token);

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
                            streamId = session.StreamId,
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
        catch (OperationCanceledException) when (session.Token.IsCancellationRequested)
        {
            if (!HttpContext.RequestAborted.IsCancellationRequested)
            {
                await SseResponseWriter.WriteEventAsync(
                    Response,
                    "stopped",
                    new { streamId = session.StreamId, reason = "stop-request" },
                    CancellationToken.None);
            }
        }
        catch (AiChatProviderException exception)
        {
            if (!HttpContext.RequestAborted.IsCancellationRequested)
            {
                await SseResponseWriter.WriteEventAsync(
                    Response,
                    "error",
                    new { streamId = session.StreamId, message = exception.Message },
                    CancellationToken.None);
            }
        }
        finally
        {
            _streamSessions.CompleteSession(session.StreamId);
        }
    }

    [HttpPost("stream/stop")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult StopStream([FromBody] ChatStreamStopRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        return _streamSessions.TryStopSession(userId, request.StreamId)
            ? NoContent()
            : NotFound();
    }

    private string? GetCurrentUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
}
