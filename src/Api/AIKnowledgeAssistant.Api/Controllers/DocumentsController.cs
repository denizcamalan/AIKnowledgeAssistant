using AIKnowledgeAssistant.Api.Contracts.Documents;
using AIKnowledgeAssistant.Application.Documents;
using AIKnowledgeAssistant.Domain.Documents;
using Microsoft.AspNetCore.Mvc;

namespace AIKnowledgeAssistant.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService) => _documentService = documentService;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DocumentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DocumentSummaryDto>>> List(CancellationToken cancellationToken)
    {
        var documents = await _documentService.ListAsync(cancellationToken);
        return Ok(documents.Select(DocumentDtoMapping.ToDto).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DocumentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DocumentDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var document = await _documentService.GetByIdAsync(id, cancellationToken);
            return Ok(DocumentDtoMapping.ToDto(document));
        }
        catch (DocumentNotFoundException ex)
        {
            return NotFoundProblem(ex.Message);
        }
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(DocumentDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DocumentDetailDto>> Upload(
        IFormFile? file,
        [FromForm] string? displayName,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["file"] = ["A file is required."],
            }));
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var document = await _documentService.UploadAsync(
                stream,
                file.FileName,
                file.ContentType,
                file.Length,
                displayName,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = document.Id },
                DocumentDtoMapping.ToDto(document));
        }
        catch (DuplicateDocumentException ex)
        {
            return ConflictProblem(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequestProblem(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(DocumentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DocumentDetailDto>> Update(
        Guid id,
        [FromBody] UpdateDocumentRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var document = await _documentService.UpdateAsync(id, request.DisplayName, cancellationToken);
            return Ok(DocumentDtoMapping.ToDto(document));
        }
        catch (DocumentNotFoundException ex)
        {
            return NotFoundProblem(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequestProblem(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _documentService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (DocumentNotFoundException ex)
        {
            return NotFoundProblem(ex.Message);
        }
    }

    private ObjectResult NotFoundProblem(string detail) =>
        Problem(statusCode: StatusCodes.Status404NotFound, title: "Not Found", detail: detail);

    private ObjectResult ConflictProblem(string detail) =>
        Problem(statusCode: StatusCodes.Status409Conflict, title: "Conflict", detail: detail);

    private ObjectResult BadRequestProblem(string detail) =>
        Problem(statusCode: StatusCodes.Status400BadRequest, title: "Bad Request", detail: detail);
}
