using AIKnowledgeAssistant.Application.Auth;
using AIKnowledgeAssistant.Application.Chat;
using AIKnowledgeAssistant.Domain.Documents;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Hosting;

namespace AIKnowledgeAssistant.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;
    private readonly ProblemDetailsFactory _problemDetailsFactory;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment,
        ProblemDetailsFactory problemDetailsFactory)
    {
        _logger = logger;
        _environment = environment;
        _problemDetailsFactory = problemDetailsFactory;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            _logger.LogDebug(
                "Request aborted before a response was written. TraceId={TraceId}",
                httpContext.TraceIdentifier);
            return false;
        }

        var mapping = MapException(exception);
        var traceId = httpContext.TraceIdentifier;

        LogException(exception, mapping.StatusCode, traceId);

        var problemDetails = _problemDetailsFactory.CreateProblemDetails(
            httpContext,
            mapping.StatusCode,
            title: mapping.Title,
            type: $"https://httpstatuses.com/{mapping.StatusCode}",
            detail: ResolveDetail(exception, mapping));

        problemDetails.Extensions["traceId"] = traceId;

        httpContext.Response.StatusCode = mapping.StatusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private string ResolveDetail(Exception exception, ExceptionMapping mapping)
    {
        if (mapping.IsClientError)
        {
            return exception.Message;
        }

        return _environment.IsDevelopment()
            ? exception.Message
            : "An unexpected error occurred. Use the traceId when contacting support.";
    }

    private void LogException(Exception exception, int statusCode, string traceId)
    {
        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unhandled exception. Status={StatusCode} TraceId={TraceId}",
                statusCode,
                traceId);
            return;
        }

        _logger.LogWarning(
            exception,
            "Request failed. Status={StatusCode} TraceId={TraceId}",
            statusCode,
            traceId);
    }

    private static ExceptionMapping MapException(Exception exception) =>
        exception switch
        {
            DocumentNotFoundException => new ExceptionMapping(
                StatusCodes.Status404NotFound,
                "Not Found",
                IsClientError: true),
            DuplicateDocumentException => new ExceptionMapping(
                StatusCodes.Status409Conflict,
                "Conflict",
                IsClientError: true),
            ArgumentException => new ExceptionMapping(
                StatusCodes.Status400BadRequest,
                "Bad Request",
                IsClientError: true),
            InvalidCredentialsException => new ExceptionMapping(
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                IsClientError: true),
            AiChatProviderException => new ExceptionMapping(
                StatusCodes.Status502BadGateway,
                "Bad Gateway",
                IsClientError: true),
            _ => new ExceptionMapping(
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                IsClientError: false),
        };

    private sealed record ExceptionMapping(int StatusCode, string Title, bool IsClientError);
}
