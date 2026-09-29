using AIKnowledgeAssistant.Api.Contracts.Chat;
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

    public ChatController(IAiChatService chatService) => _chatService = chatService;

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
        });
    }
}
