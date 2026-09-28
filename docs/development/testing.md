# Testing (P1-07)

## Test pyramid

| Layer | Project | What it proves |
|-------|---------|----------------|
| **Unit** | `tests/AIKnowledgeAssistant.UnitTests` | Application rules (`DocumentService`), infrastructure edge cases (`LocalFileStorage`), API helpers (`GlobalExceptionHandler`, labs) without I/O. |
| **Integration** | `tests/AIKnowledgeAssistant.IntegrationTests` | Real HTTP pipeline via `WebApplicationFactory<Program>` — routing, middleware, serialization, validation. |

Prefer many fast unit tests for business rules; use integration tests for wiring and status codes.

## Run everything (default CI / local)

```bash
dotnet test src/AIKnowledgeAssistant.slnx
```

This runs all tests that do **not** require Docker PostgreSQL. Document CRUD flows marked with `[PostgresFact]` are skipped unless opted in.

## PostgreSQL integration tests

1. Start local Postgres (see repo `docker/` compose).
2. Export:

```bash
export AKA_RUN_POSTGRES_TESTS=1
# optional override:
# export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=aiknowledgeassistant;Username=aka;Password=aka_dev_password"
dotnet test src/AIKnowledgeAssistant.slnx
```

`CustomWebApplicationFactory` applies migrations before `[PostgresFact]` tests and uses an isolated temp upload folder per factory instance.

## Conventions

- **xUnit** — `[Fact]` for single cases; `IClassFixture<CustomWebApplicationFactory>` for shared API host.
- **Fakes over mocks** — hand-written `FakeDocumentRepository` / `FakeFileStorage` for `DocumentService` keep tests readable; reserve mocking libraries for awkward externals later.
- **No secrets in tests** — connection strings come from environment or documented dev defaults.

## Related

- [ADR-002: Test strategy](../adr/ADR-002-test-strategy.md)
- [Global exception handling](global-exception-handling.md)
