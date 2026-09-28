# Async and cancellation (P1-06)

Learning endpoint: `GET /api/labs/async?ioDelayMs=25`

`ioDelayMs` is clamped to 0–500. The document API already passes `CancellationToken` from the controller through `DocumentService` into EF Core and `LocalFileStorage.SaveAsync`.

## I/O-bound vs CPU-bound

| Kind | What the lab does | Thread |
|------|-------------------|--------|
| **I/O-bound** | `await Task.Delay(..., cancellationToken)` | Released while waiting. The token cancels the wait. |
| **CPU-bound** | `await Task.Run(...)` plus `ThrowIfCancellationRequested` inside the loop | A thread-pool thread runs the loop. `Task.Run`'s token only skips starting the delegate; the loop must observe the token itself. |

`Task.Run` is for CPU work. Wrapping `CopyToAsync` or `SaveChangesAsync` in `Task.Run` still occupies a thread and does not make I/O faster.

## Cancellation is cooperative

`CancellationToken` does not abort a thread. Each layer has to pass the token into an API that accepts it, or poll it. The HTTP request token is `HttpContext.RequestAborted` (the action parameter named `cancellationToken`).

Document path:

`DocumentsController` → `DocumentService` → `EfDocumentRepository` (`SaveChangesAsync` / queries) and `LocalFileStorage.SaveAsync` (`CopyToAsync`).

`DeleteAsync` calls `ThrowIfCancellationRequested` before `File.Delete`. There is no `File.DeleteAsync`; the delete itself stays synchronous and can block a thread for the duration of the filesystem call.

## Sync-over-async (created, then removed)

This blocks the caller and can deadlock if a `SynchronizationContext` is waiting for the same thread:

```csharp
// Removed. Do not put this on a request path.
var sample = AsyncWork.RunIoBoundAsync(25, cancellationToken).GetAwaiter().GetResult();
```

The lab and the document pipeline `await` instead. A unit test cancels the token and expects `OperationCanceledException`.

## ConfigureAwait

`ConfigureAwait(false)` avoids resuming on a captured sync context. ASP.NET Core request code does not have a `SynchronizationContext`, so controllers and services in this app omit it. Use it in reusable libraries that may run on UI or classic ASP.NET.

## Client disconnect

`GlobalExceptionHandler` does not map an aborted request to ProblemDetails. See [global-exception-handling.md](global-exception-handling.md).
