# Global exception handling (P1-05)

## Pipeline

1. `AddApiExceptionHandling()` — registers `GlobalExceptionHandler` + `ProblemDetails` customization.
2. `app.UseExceptionHandler()` — runs early in the middleware pipeline (before HTTPS redirection).

Controller actions do not catch domain exceptions; `GlobalExceptionHandler` maps them to RFC 7807-style JSON.

## Mapped exceptions

| Exception | HTTP status |
|-----------|-------------|
| `DocumentNotFoundException` | 404 |
| `DuplicateDocumentException` | 409 |
| `ArgumentException` (validation in services) | 400 |
| Anything else | 500 |

## Response shape

All problem responses include extension `traceId` (from `HttpContext.TraceIdentifier`), including automatic model-validation `ValidationProblemDetails`.

## Production vs Development

- **4xx (mapped domain):** `detail` = exception message (safe for clients).
- **500:** Production returns a generic detail string; Development includes the exception message. Stack traces are never written to the response body.
- **Logging:** 4xx mapped failures → `LogWarning`; unhandled 500 → `LogError` with full exception (server logs only).

## Lab endpoint

Development/Production: `GET /api/labs/errors/unhandled` throws intentionally for manual verification of 500 handling.
