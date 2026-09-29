using AIKnowledgeAssistant.Application.Configuration;
using AIKnowledgeAssistant.Application.Documents;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AIKnowledgeAssistant.Infrastructure.Documents;

internal sealed class LocalFileStorage : IFileStorage
{
    private readonly DocumentStorageOptions _options;
    private readonly string _rootPath;

    public LocalFileStorage(IOptions<DocumentStorageOptions> options, IHostEnvironment environment)
    {
        _options = options.Value;
        _rootPath = Path.IsPathRooted(_options.RootPath)
            ? _options.RootPath
            : Path.Combine(environment.ContentRootPath, _options.RootPath);
    }

    public async Task<string> SaveAsync(
        Guid documentId,
        Stream content,
        string fileName,
        CancellationToken cancellationToken)
    {
        var relativeDirectory = Path.Combine(documentId.ToString("N"));
        var absoluteDirectory = Path.Combine(_rootPath, relativeDirectory);
        Directory.CreateDirectory(absoluteDirectory);

        var safeFileName = Path.GetFileName(fileName);
        var absolutePath = Path.Combine(absoluteDirectory, safeFileName);

        await using var fileStream = new FileStream(
            absolutePath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 4096,
            useAsync: true);

        await content.CopyToAsync(fileStream, cancellationToken);

        return Path.Combine(relativeDirectory, safeFileName).Replace('\\', '/');
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken)
    {
        // File.Delete has no asynchronous API. Observe cancellation before the blocking call
        // so an aborted request does not delete a file the caller already gave up on.
        cancellationToken.ThrowIfCancellationRequested();

        var absolutePath = Path.Combine(_rootPath, storagePath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }

        var directory = Path.GetDirectoryName(absolutePath);
        if (directory is not null && Directory.Exists(directory) && !Directory.EnumerateFileSystemEntries(directory).Any())
        {
            Directory.Delete(directory);
        }

        return Task.CompletedTask;
    }

    public Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var absolutePath = Path.Combine(_rootPath, storagePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(absolutePath))
        {
            throw new FileNotFoundException("Document file was not found on disk.", absolutePath);
        }

        Stream stream = new FileStream(
            absolutePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);
        return Task.FromResult(stream);
    }
}
