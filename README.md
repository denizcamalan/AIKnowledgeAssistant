# AI Knowledge Assistant

A learning and portfolio project: upload PDF/TXT documents, index them with embeddings, and ask questions answered **with citations** over **server-sent streaming** (SSE). Backend: **.NET 8** modular monolith. Frontend: **React 18 + TypeScript** (P2).

## Features

**MVP (P2–P5):** document upload, text extraction, chunking, pgvector search, grounded RAG answers, citations, SSE streaming, JWT auth, chat UI.

**Later:** hybrid search & reranking (P6), Redis / MongoDB / RabbitMQ (P7), Semantic Kernel (P8), agents (P9), production hardening (P10), CI/CD & cloud (P11), portfolio docs (P12).

Task tracking: [Notion — AI Knowledge Assistant](https://app.notion.com/p/3e578ebbe6d1813892e5d1da0c300887).

## Architecture

Modular monolith: one API process, modules **Auth**, **Documents**, **Ingestion**, **Retrieval**, **Chat**. Details: [docs/architecture/overview.md](docs/architecture/overview.md).

Decisions: [docs/adr/README.md](docs/adr/README.md).

## Tech stack

| Technology | Role in this project |
|------------|----------------------|
| ASP.NET Core 8 | REST API, SSE, JWT (P2) |
| PostgreSQL + pgvector | Users, documents, chunks, vectors |
| MongoDB | Chat history & audit (P7) |
| Redis | Retrieval/cache (P7) |
| RabbitMQ | Async document indexing (P7) |
| React + TypeScript | Chat and document UI (P2) |
| Docker Compose | Local infrastructure (P0) |

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/) (for local databases)
- Node.js 20+ (when `client/web` is added in P2)

## Quick start

### 1. Infrastructure

```bash
cp docker/.env.example docker/.env   # edit passwords for local use
docker compose -f docker/docker-compose.yml up -d
docker compose -f docker/docker-compose.yml ps
```

Services: PostgreSQL (5432), MongoDB (27017), Redis (6379), RabbitMQ (5672, management UI 15672).

### 2. API

```bash
dotnet build src/AIKnowledgeAssistant.slnx
dotnet run --project src/Api/AIKnowledgeAssistant.Api
```

- Swagger (Development): `https://localhost:7xxx/swagger` (see launchSettings)
- Health: `GET /health`

### 3. Configuration

Application settings live under `src/Api/AIKnowledgeAssistant.Api/appsettings*.json`. Connection strings and API keys will be added in P1+; use user secrets or environment variables—never commit secrets.

## API overview (planned)

| Area | Endpoints (phases) |
|------|---------------------|
| Health | `GET /health` (P1) |
| Auth | register/login, JWT (P2) |
| Documents | upload, list, delete (P1–P5) |
| Chat | RAG + SSE (P4–P5) |

## RAG pipeline (summary)

1. **Ingest:** extract text → chunk + metadata  
2. **Index:** embed chunks → store in pgvector  
3. **Retrieve:** embed query → top-k (user-scoped)  
4. **Generate:** grounded prompt → LLM stream  
5. **Cite:** map answer to chunk/document ids for the UI  

## Development workflow

Aligned with Notion tasks (P0–P12):

1. **Learn** — understand the topic  
2. **Analyze** — review existing code  
3. **Plan** — agree on a small implementation plan  
4. **Implement** — incremental changes  
5. **Test** — unit / integration as appropriate  
6. **Explain** — document what changed and why  
7. **Done** — acceptance criteria + learning notes + interview prep  

### Cursor rules

- Start from the task’s **Cursor Prompt** in Notion when available.  
- Ask for analysis and a plan before large refactors.  
- Prefer small PRs; update README or ADRs when decisions change.  
- Do not commit `.env` or API keys.  

## Testing

```bash
dotnet test src/AIKnowledgeAssistant.slnx
```

Integration tests against Dockerized PostgreSQL will expand in P1-07.

## Repository layout

```
src/AIKnowledgeAssistant.slnx
src/Api/              # HTTP host
src/Application/      # use cases, interfaces
src/Domain/           # entities, domain rules
src/Infrastructure/   # EF, external services
tests/                # unit + integration
docker/               # Compose for local infra
docs/adr/             # architecture decisions
client/web/           # React app (P2)
```

## Build policy

Root [Directory.Build.props](Directory.Build.props): nullable reference types enabled, **warnings treated as errors**, latest analysis level.

## Roadmap

| Phase | Focus |
|-------|--------|
| P0 | Foundation, Docker, solution, ADRs |
| P1 | Web API, PostgreSQL, Document API, tests |
| P2 | JWT + React shell |
| P3–P4 | LLM + SSE |
| P5 | RAG MVP |
| P6–P12 | Quality, infra, SK, agents, prod, CI/CD |

## Learning outcomes

Maps to job-style skills: C# / .NET Web API, async & SSE, PostgreSQL/pgvector, JWT & React, prompt/RAG, distributed pieces (Redis, Mongo, RabbitMQ), Semantic Kernel & agents, TDD, Docker, CI/CD.

## License

See [LICENSE](LICENSE).
