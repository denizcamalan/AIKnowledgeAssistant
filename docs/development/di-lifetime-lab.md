# DI lifetime lab (P1-04)

Interactive endpoint (Development only): `GET /api/labs/di`

## What it shows

| Lifetime | Registration | Within one HTTP request | Across HTTP requests |
|----------|--------------|-------------------------|----------------------|
| **Transient** | `AddTransient` | New instance per injection / `GetRequiredService` | N/A (not cached) |
| **Scoped** | `AddScoped` | Same instance | New instance per request |
| **Singleton** | `AddSingleton` | Same instance | Same instance (app lifetime) |

The response compares **constructor injection** with a second **service resolution** via `IServiceProvider.GetRequiredService` in the same request.

`RequestScopedClock` is a scoped stand-in for per-request state (similar to `DbContext`).

## Captive dependency

Do **not** inject scoped services into singletons. Example anti-pattern:

```csharp
services.AddScoped<RequestScopedClock>();
services.AddSingleton<CaptiveDependencyConsumer>(); // ctor takes RequestScopedClock
```

With `ValidateScopes = true`, building the container throws:

`Cannot consume scoped service ... from singleton ...`

See `CaptiveDependencyTests` in unit tests.

## Production registrations (reference)

- `IDocumentService` → Scoped  
- `IDocumentRepository` / `AppDbContext` → Scoped  
- `IFileStorage` → Singleton (stateless path config; safe with singleton `IOptions` / `IHostEnvironment`)
