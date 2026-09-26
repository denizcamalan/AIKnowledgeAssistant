# ADR-0001: Record architecture decisions

## Status

Accepted

## Context

This is a learning and portfolio project with many technology choices across P0–P12. We need a lightweight way to capture *why* a decision was made, what we considered, and what changes if we revisit it.

## Decision

Use short Markdown ADRs in `docs/adr/` named `NNNN-short-title.md`, each containing:

- **Status** (Proposed | Accepted | Superseded)
- **Context**
- **Decision**
- **Consequences** (positive and negative)

## Consequences

- Trade-offs are visible in code review and interviews.
- Some ADRs will be written before implementation (P0) and refined when code lands (P1+).
