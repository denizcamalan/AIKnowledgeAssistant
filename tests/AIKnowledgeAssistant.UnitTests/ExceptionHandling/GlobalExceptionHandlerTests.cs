using System.Text.Json;
using AIKnowledgeAssistant.Api.ExceptionHandling;
using AIKnowledgeAssistant.Domain.Documents;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace AIKnowledgeAssistant.UnitTests.ExceptionHandling;

public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_DocumentNotFound_Returns404ProblemDetailsWithTraceId()
    {
        var handler = CreateHandler(environments: Environments.Development);
        var context = new DefaultHttpContext { Response = { Body = new MemoryStream() } };
        context.TraceIdentifier = "trace-404";
        context.RequestServices = CreateServices();

        var handled = await handler.TryHandleAsync(
            context,
            new DocumentNotFoundException(Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);

        var problem = await ReadProblemDetailsAsync(context);
        Assert.Equal("Not Found", problem.GetProperty("title").GetString());
        Assert.Equal("trace-404", problem.GetProperty("traceId").GetString());
    }

    [Fact]
    public async Task TryHandleAsync_UnhandledException_InProduction_HidesInternalDetail()
    {
        var handler = CreateHandler(environments: Environments.Production);
        var context = new DefaultHttpContext { Response = { Body = new MemoryStream() } };
        context.TraceIdentifier = "trace-500";
        context.RequestServices = CreateServices();

        await handler.TryHandleAsync(
            context,
            new InvalidOperationException("Sensitive database password leaked"),
            CancellationToken.None);

        var problem = await ReadProblemDetailsAsync(context);
        Assert.Equal(500, problem.GetProperty("status").GetInt32());
        Assert.DoesNotContain("Sensitive", problem.GetProperty("detail").GetString());
        Assert.Equal("trace-500", problem.GetProperty("traceId").GetString());
    }

    private static GlobalExceptionHandler CreateHandler(string environments)
    {
        var environment = new HostEnvironmentStub(environments);
        var factory = new TestProblemDetailsFactory();
        return new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance, environment, factory);
    }

    private static IServiceProvider CreateServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ProblemDetailsFactory, TestProblemDetailsFactory>();
        return services.BuildServiceProvider();
    }

    private static async Task<JsonElement> ReadProblemDetailsAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        return await JsonSerializer.DeserializeAsync<JsonElement>(context.Response.Body);
    }

    private sealed class HostEnvironmentStub(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;

        public string ApplicationName { get; set; } = "Test";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private sealed class TestProblemDetailsFactory : ProblemDetailsFactory
    {
        public override ProblemDetails CreateProblemDetails(
            HttpContext httpContext,
            int? statusCode = null,
            string? title = null,
            string? type = null,
            string? detail = null,
            string? instance = null) =>
            new()
            {
                Status = statusCode,
                Title = title,
                Type = type,
                Detail = detail,
                Instance = instance,
            };

        public override ValidationProblemDetails CreateValidationProblemDetails(
            HttpContext httpContext,
            ModelStateDictionary modelStateDictionary,
            int? statusCode = null,
            string? title = null,
            string? type = null,
            string? detail = null,
            string? instance = null) =>
            new(modelStateDictionary)
            {
                Status = statusCode,
                Title = title,
                Type = type,
                Detail = detail,
                Instance = instance,
            };
    }
}
