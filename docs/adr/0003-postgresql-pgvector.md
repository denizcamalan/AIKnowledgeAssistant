# ADR-0003: PostgreSQL + pgvector for documents and vectors

## Status

Accepted

## Context

We need transactional metadata (users, documents, chunks) and vector similarity search with filters (per user, per document). A separate vector database adds operational cost for a small learning dataset.

## Decision

Use **PostgreSQL** as the system of record and **pgvector** for chunk embeddings. Apply indexes (e.g. HNSW) when data volume warrants it (P5+).

## Consequences

- **Positive:** ACID, joins, one backup story; good fit for MVP RAG.
- **Negative:** Very large-scale vector workloads may eventually need a dedicated store or sharding—we keep retrieval behind an interface.
