# Architecture Decision Records (ADR)

Significant technical choices for AI Knowledge Assistant are recorded here so we can explain **why** the system looks the way it does (reviews, portfolio, interviews).

## Workflow

1. Copy [TEMPLATE.md](TEMPLATE.md) to a new file.
2. Fill every section; set status to **Proposed** until reviewed.
3. Link related Notion tasks and PRs.
4. Set status to **Accepted** when the team agrees (solo: after Plan step in [AI coding guidelines](../development/ai-coding-guidelines.md)).

## Numbering standard

Use sequential, zero-padded numbers and a kebab-case slug:

```text
docs/adr/ADR-001-modular-monolith.md
docs/adr/ADR-002-postgresql-pgvector.md
```

| Rule | Detail |
|------|--------|
| Prefix | `ADR-` (uppercase) |
| Number | Three digits: `001`, `002`, … |
| Slug | Lowercase, hyphen-separated, matches the decision topic |
| One decision per file | Split or supersede rather than mixing unrelated choices |

When an ADR is replaced, set status to **Superseded by ADR-NNN** and add a line at the top of the old file pointing to the new one.

## Index

| ADR | Title | Status |
|-----|-------|--------|
| [ADR-001](ADR-001-modular-monolith.md) | Modular monolith for the API host | Accepted |
| [ADR-002](ADR-002-test-strategy.md) | xUnit unit + WebApplicationFactory integration; Postgres opt-in | Accepted |

### Legacy P0 drafts (superseded for new work)

Early bootstrap files used a `000N-` prefix. Prefer **ADR-NNN** for new decisions. Content may still be useful until migrated:

| File | Topic |
|------|--------|
| [0001](0001-record-architecture-decisions.md) | ADR process (see TEMPLATE + this README) |
| [0002](0002-modular-monolith.md) | Short monolith note → see **ADR-001** |
| [0003](0003-postgresql-pgvector.md) | PostgreSQL + pgvector |
| [0004](0004-jwt-bearer-auth.md) | JWT bearer auth |
| [0005](0005-rag-before-semantic-kernel.md) | RAG before Semantic Kernel |

Planned ADRs: chunking (P5), SSE vs WebSockets (P4), RabbitMQ ingestion (P7), MongoDB chat history (P7), file storage (P1).

## Related

- [AI coding guidelines](../development/ai-coding-guidelines.md)
- [Architecture overview](../architecture/overview.md)
