# ADR-0002: Modular monolith

## Status

Accepted

## Context

The product needs upload → ingest → RAG → SSE streaming end-to-end early. Splitting into microservices would add deployment and debugging overhead before the domain is understood.

## Decision

Ship a **single deployable** ASP.NET Core API (`AIKnowledgeAssistant.Api`) with modules in `Application` / `Infrastructure` (Auth, Documents, Ingestion, Retrieval, Chat). Use interfaces at module boundaries so a worker or service can be extracted later (e.g. ingestion consumer in P7).

## Consequences

- **Positive:** Fast local dev, simpler integration tests, one CI artifact.
- **Negative:** All modules scale together until we split; discipline is required to avoid a “big ball of mud.”
