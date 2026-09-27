using AIKnowledgeAssistant.Application.Configuration;
using AIKnowledgeAssistant.Infrastructure.Documents;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AIKnowledgeAssistant.UnitTests.Documents;

public sealed class LocalFileStorageTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "aka-async", Guid.NewGuid().ToString("N"));

    public LocalFileStorageTests() => Directory.CreateDirectory(_root);

    [Fact]
    public async Task DeleteAsync_WhenCancelled_LeavesTheFileOnDisk()
    {
        var storage = CreateStorage();
        var relativePath = await storage.SaveAsync(
            Guid.NewGuid(),
            new MemoryStream("hello"u8.ToArray()),
            "note.txt",
            CancellationToken.None);
        var absolutePath = Path.Combine(_root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(absolutePath));

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var exception = await Record.ExceptionAsync(() => storage.DeleteAsync(relativePath, cts.Token));

        Assert.IsAssignableFrom<OperationCanceledException>(exception);
        Assert.True(File.Exists(absolutePath));
    }

    [Fact]
    public async Task DeleteAsync_RemovesTheFile()
    {
        var storage = CreateStorage();
        var relativePath = await storage.SaveAsync(
            Guid.NewGuid(),
            new MemoryStream("hello"u8.ToArray()),
            "note.txt",
            CancellationToken.None);

        await storage.DeleteAsync(relativePath, CancellationToken.None);

        var absolutePath = Path.Combine(_root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Assert.False(File.Exists(absolutePath));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private LocalFileStorage CreateStorage() =>
        new(Options.Create(new DocumentStorageOptions { RootPath = _root }), new HostEnvironmentStub(_root));

    private sealed class HostEnvironmentStub(string contentRootPath) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;

        public string ApplicationName { get; set; } = "Tests";

        public string ContentRootPath { get; set; } = contentRootPath;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
