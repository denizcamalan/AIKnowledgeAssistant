using AIKnowledgeAssistant.Api.Contracts.Documents;
using AIKnowledgeAssistant.Application.Documents;
using AIKnowledgeAssistant.Application.Ingestion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIKnowledgeAssistant.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly IDocumentInsightService _documentInsightService;
    private readonly IDocumentIngestionPipeline _ingestionPipeline;
    private readonly IDocumentChunkService _documentChunks;

    public DocumentsController(
        IDocumentService documentService,
        IDocumentInsightService documentInsightService,
        IDocumentIngestionPipeline ingestionPipeline,
        IDocumentChunkService documentChunks)
    {
        _documentService = documentService;
        _documentInsightService = documentInsightService;
        _ingestionPipeline = ingestionPipeline;
        _documentChunks = documentChunks;
    }

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
        var document = await _documentService.GetByIdAsync(id, cancellationToken);
        return Ok(DocumentDtoMapping.ToDto(document));
    }

    [HttpPost("{id:guid}/classify")]
    [Authorize]
    [ProducesResponseType(typeof(DocumentClassificationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<DocumentClassificationResponseDto>> Classify(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _documentInsightService.ClassifyAsync(id, cancellationToken);
        return Ok(DocumentDtoMapping.ToDto(result));
    }

    [HttpGet("{id:guid}/chunks")]
    [ProducesResponseType(typeof(IReadOnlyList<DocumentChunkDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<DocumentChunkDto>>> ListChunks(Guid id, CancellationToken cancellationToken)
    {
        var chunks = await _documentChunks.ListByDocumentIdAsync(id, cancellationToken);
        return Ok(chunks.Select(DocumentDtoMapping.ToDto).ToList());
    }

    [HttpPost("{id:guid}/ingestion/run")]
    [ProducesResponseType(typeof(DocumentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DocumentDetailDto>> RunIngestion(Guid id, CancellationToken cancellationToken)
    {
        var document = await _ingestionPipeline.RunAsync(id, cancellationToken);
        return Ok(DocumentDtoMapping.ToDto(document));
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

        var document = await _documentService.UpdateAsync(id, request.DisplayName, cancellationToken);
        return Ok(DocumentDtoMapping.ToDto(document));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _documentService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
