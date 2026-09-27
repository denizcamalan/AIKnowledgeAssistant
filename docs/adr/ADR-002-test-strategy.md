# ADR-002: Test strategy for P1

## Title

Use xUnit with a small unit suite and `WebApplicationFactory` integration tests; gate PostgreSQL-dependent tests behind an environment flag.

## Status

Accepted

## Date

2026-09-27

## Context

P1 delivers document CRUD, EF Core persistence, global error handling, and lab endpoints. We need confidence in business rules and HTTP behavior without blocking every `dotnet test` on Docker.

## Decision

1. **Unit tests** target `Application` services and selected `Infrastructure` / API helpers with fakes or real instances where cheap.
2. **Integration tests** host the real API (`Program`) through `CustomWebApplicationFactory`, asserting status codes and JSON contracts.
3. **Database integration** uses `[PostgresFact]` and `AKA_RUN_POSTGRES_TESTS=1` so default test runs stay fast and CI-friendly until P10 adds containers.

## Consequences

- Developers must opt in to run full document E2E against Postgres locally.
- Testcontainers and broader coverage are explicitly deferred to P10-02.

## Related

- [Testing guide](../development/testing.md)
- Notion: P1-07, P10-02
