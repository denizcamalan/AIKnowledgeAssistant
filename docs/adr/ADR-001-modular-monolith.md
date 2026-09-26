# ADR-001: Modular monolith for the API host

## Title

Adopt a .NET 8 modular monolith as the initial deployment and code structure for AI Knowledge Assistant.

## Status

Accepted

## Date

2026-09-26

## Context

AI Knowledge Assistant is a learning and portfolio project. The product path is: upload PDF/TXT → index (chunk + embed) → RAG Q&A with citations → SSE streaming, with JWT and a React client in early phases. The team is small (effectively one developer), the dataset is modest, and the priority is to **learn end-to-end behavior** (RAG, streaming, persistence) without operational sprawl.

The codebase is organized as `Api`, `Application`, `Domain`, and `Infrastructure` with logical modules: Auth, Documents, Ingestion, Retrieval, and Chat.

## Problem

We need a deployment and structuring model that:

- Delivers a working vertical slice quickly (P1–P5).
- Keeps module boundaries clear enough to extract services later if needed.
- Avoids premature distributed-system complexity that distracts from RAG and .NET fundamentals.

## Options Considered

| Option | Summary |
|--------|---------|
| **A. Modular monolith** | Single deployable ASP.NET Core API; modules as folders and interfaces inside one solution. |
| **B. Microservices from day one** | Separate services for auth, documents, ingestion, chat/LLM, each with own DB and deploy pipeline. |
| **C. “Monolith first, split later” without modules** | One project, minimal layering; fast start but weak boundaries. |

## Decision

Use **option A: modular monolith** — one primary API process (`AIKnowledgeAssistant.Api`) with clear module boundaries in `Application` / `Infrastructure`, and interfaces at edges where a future worker or service might attach (e.g. ingestion consumer in P7).

## Rationale

- **Learning focus:** RAG, SSE, EF Core, pgvector, and JWT are the syllabus; operating five services locally does not advance that goal in P1–P5.
- **Speed of feedback:** One `dotnet run`, one integration test host, one CI artifact — faster Learn → Test loops.
- **Honest boundaries:** Feature folders and interfaces document where a split *could* happen without paying microservice costs now.
- **Option C** was rejected because this project explicitly uses Clean-ish layering and ADRs; a single unstructured project would make later extraction harder.

## Consequences

### Positive

- Simple local and CI workflows; easier debugging across upload → retrieve → stream.
- Shared transaction boundaries where useful (e.g. document metadata + chunk rows in PostgreSQL).
- Module interfaces can become HTTP or queue contracts when extracting a worker or gateway.

### Negative

- All modules scale and deploy together until split; a hot path (e.g. ingestion) cannot scale independently yet.
- Requires discipline: no “reach across” modules without going through `Application` abstractions.
- Risk of coupling if modules share DTOs or DbContext usage without clear ownership.

## Alternatives Rejected

| Alternative | Why rejected |
|-------------|--------------|
| **Microservices from day one** | High overhead: service discovery, distributed tracing, contract versioning, multiple databases, and failure modes — before domain and RAG pipeline are stable. No organizational or scale trigger yet. |
| **Unlayered monolith** | Faster for a spike but conflicts with project learning goals (DI, testing, ADRs) and makes P7 worker extraction messier. |

## When a module might become a separate service

Revisit this ADR (or add ADR-00X) if **one or more** of these appear:

| Module / capability | Typical trigger |
|---------------------|-----------------|
| **Ingestion worker** | Long-running PDF parse + embed batches; need queue (RabbitMQ), retries, DLQ, scale-out workers (P7). |
| **Retrieval / embedding** | Separate SLA, GPU hosting, or model versioning isolated from API latency. |
| **Chat / LLM gateway** | Centralized rate limits, billing, model routing, or compliance boundary. |
| **Auth** | Shared identity across multiple products (OAuth/OIDC platform) — not required for MVP. |

Extraction should start with **the same interface** the monolith already uses (e.g. `IIngestionQueue`, `ILlmClient`), then move implementation behind a network or queue boundary — not a big-bang rewrite.

## Trade-offs (summary)

| Dimension | Modular monolith (chosen) | Early microservices |
|-----------|---------------------------|---------------------|
| Time to MVP | Low | High |
| Operational complexity | Low | High |
| Independent scaling | Limited | High |
| Learning RAG/.NET on one codebase | Strong | Diluted |
| Refactor cost to split later | Moderate (if boundaries kept) | Lower per service, higher upfront |

## Related Tasks

- Notion: P0-01 (scope), P0-03 (solution layout), P0-04 (ADR process)
- Supersedes / extends the short P0 draft [0002-modular-monolith.md](0002-modular-monolith.md); **ADR-001 is the canonical record** for this decision going forward.
