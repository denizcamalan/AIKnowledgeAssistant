# Architecture Overview

AI Knowledge Assistant is a **modular monolith**: one deployable ASP.NET Core API with clear module boundaries inside `Application` and `Infrastructure`. A React + TypeScript client talks to the API over REST (JWT) and SSE for streamed answers.

## Product flow

1. User signs in (JWT).
2. User uploads PDF or TXT documents.
3. Ingestion extracts text, chunks content, generates embeddings, stores vectors in PostgreSQL (pgvector).
4. User asks a question in chat.
5. Retrieval embeds the query, runs vector search (top-k, scoped to the user’s documents).
6. Chat builds a grounded prompt, streams the LLM response via SSE, and returns citations to source chunks.

Later phases add hybrid search, reranking, Redis cache, MongoDB chat history, RabbitMQ async ingestion, Semantic Kernel, and agents—without changing the core module boundaries.

## Logical modules

| Module | Responsibility |
|--------|----------------|
| **Auth** | Users, JWT issue/validation, authorization claims |
| **Documents** | Upload metadata, file storage, indexing status |
| **Ingestion** | Extract → chunk → embed → persist |
| **Retrieval** | Query embedding, vector search, (P6) hybrid/rerank |
| **Chat** | RAG orchestration, SSE streaming, citations |
| **Api** | HTTP surface, ProblemDetails, OpenAPI |

## Data stores (by phase)

| Store | Role | Phase |
|-------|------|-------|
| PostgreSQL + pgvector | Users, documents, chunks, embeddings | P1 / P5 |
| Local/blob file storage | Raw uploads | P1 |
| MongoDB | Chat history, audit | P7 |
| Redis | Cache (retrieval, embeddings) | P7 |
| RabbitMQ | Async document indexing | P7 |

## Future service boundaries

Extraction candidates if scale demands it: **Ingestion Worker** (queue consumer), **LLM gateway**, **Retrieval service**. Interfaces in `Application` should keep these splits possible without rewriting domain logic.

## Phase dependency (summary)

`P0 → P1 → P2` and `P3 → P4 → P5` (with P1 persistence). `P5` unlocks P6–P9; P10–P12 harden and ship.

See [ADR index](../adr/README.md) for recorded decisions.
