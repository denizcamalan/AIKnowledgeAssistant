# ADR-0005: Implement RAG before Semantic Kernel

## Status

Accepted

## Context

Semantic Kernel provides orchestration, plugins, and agents, but RAG fundamentals (chunking, retrieval, context limits, grounding) are easier to debug without framework indirection. Some SK agent/RAG features are still evolving.

## Decision

Build the **ingest → embed → retrieve → prompt → stream** pipeline with explicit application code in P5. Introduce **Semantic Kernel** in P8 as an orchestration layer over existing abstractions (`ILlmClient`, retrieval services), not as the first RAG implementation.

## Consequences

- **Positive:** Clear mental model; SK becomes a swap-in layer.
- **Negative:** Some duplication until P8 refactors call sites into SK plugins.
