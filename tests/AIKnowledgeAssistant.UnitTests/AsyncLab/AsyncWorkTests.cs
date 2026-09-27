using AIKnowledgeAssistant.Api.AsyncLab;
using AIKnowledgeAssistant.Api.Controllers;

namespace AIKnowledgeAssistant.UnitTests.AsyncLab;

public sealed class AsyncWorkTests
{
    [Fact]
    public async Task IoBound_CompletesWithoutThreadPoolOffload()
    {
        var sample = await AsyncWork.RunIoBoundAsync(1, CancellationToken.None);

        Assert.Equal("io-bound", sample.Kind);
        Assert.False(sample.OffloadedToThreadPool);
    }

    [Fact]
    public async Task IoBound_WhenAlreadyCancelled_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var exception = await Record.ExceptionAsync(() => AsyncWork.RunIoBoundAsync(200, cts.Token));

        Assert.IsAssignableFrom<OperationCanceledException>(exception);
    }

    [Fact]
    public async Task CpuBound_WhenAlreadyCancelled_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var exception = await Record.ExceptionAsync(() => AsyncWork.RunCpuBoundAsync(cts.Token));

        Assert.IsAssignableFrom<OperationCanceledException>(exception);
    }

    [Fact]
    public async Task CpuBound_CompletesOnThreadPool()
    {
        var sample = await AsyncWork.RunCpuBoundAsync(CancellationToken.None);

        Assert.Equal("cpu-bound", sample.Kind);
        Assert.True(sample.OffloadedToThreadPool);
        Assert.NotEqual(0, sample.Result);
    }

    [Fact]
    public async Task LabEndpoint_WhenAlreadyCancelled_ThrowsInsteadOfBlocking()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var controller = new AsyncLabController();

        var exception = await Record.ExceptionAsync(() => controller.Get(200, cts.Token));

        Assert.IsAssignableFrom<OperationCanceledException>(exception);
    }
}
