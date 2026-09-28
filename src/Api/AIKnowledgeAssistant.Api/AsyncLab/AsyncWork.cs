using System.Diagnostics;

namespace AIKnowledgeAssistant.Api.AsyncLab;

/// <summary>
/// Learning samples for I/O-bound vs CPU-bound work. Sync-over-async is intentionally absent:
/// blocking with <c>GetAwaiter().GetResult()</c> or <c>.Result</c> holds a thread and can deadlock
/// when a <see cref="SynchronizationContext"/> is present.
/// </summary>
public static class AsyncWork
{
    public const int MaxIoDelayMilliseconds = 500;

    public static readonly IReadOnlyList<string> Notes =
    [
        "I/O-bound work awaits Task.Delay and passes CancellationToken; it does not block a thread.",
        "CPU-bound work uses Task.Run so the loop does not occupy the request thread. Cancellation is cooperative: the loop calls ThrowIfCancellationRequested.",
        "Sync-over-async (.Result or GetAwaiter().GetResult()) is omitted. It blocks a thread and can deadlock when a SynchronizationContext is present.",
        "ConfigureAwait(false) belongs in reusable libraries. This ASP.NET Core app has no SynchronizationContext, so request code omits it.",
    ];

    public static async Task<AsyncWorkSample> RunIoBoundAsync(int delayMilliseconds, CancellationToken cancellationToken)
    {
        var delay = TimeSpan.FromMilliseconds(Math.Clamp(delayMilliseconds, 0, MaxIoDelayMilliseconds));
        var stopwatch = Stopwatch.StartNew();
        await Task.Delay(delay, cancellationToken);
        stopwatch.Stop();

        return new AsyncWorkSample("io-bound", OffloadedToThreadPool: false, stopwatch.ElapsedMilliseconds, Result: 0);
    }

    public static async Task<AsyncWorkSample> RunCpuBoundAsync(CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var checksum = await Task.Run(() => ComputeChecksum(cancellationToken), cancellationToken);
        stopwatch.Stop();

        return new AsyncWorkSample("cpu-bound", OffloadedToThreadPool: true, stopwatch.ElapsedMilliseconds, checksum);
    }

    private static int ComputeChecksum(CancellationToken cancellationToken)
    {
        var sum = 0;
        for (var i = 0; i < 50_000; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            sum = unchecked(sum + i);
        }

        return sum;
    }
}

public sealed record AsyncWorkSample(string Kind, bool OffloadedToThreadPool, long ElapsedMilliseconds, int Result);
