# AI Coding & Development Guidelines

This repository is an **AI Knowledge Assistant** learning/portfolio project: upload and index PDF/TXT documents, answer questions with **RAG**, show **citations**, and stream responses via **SSE**. The backend is a **.NET 8 modular monolith** until architecture explicitly changes.

These rules apply to **human developers** and **AI assistants** (e.g. Cursor) working in this repo.

---

## Task workflow (Learn → Done)

Every Notion task (or equivalent work item) should follow this cycle:

| Step | Purpose |
|------|---------|
| **Learn** | Understand the concept and acceptance criteria before coding. |
| **Analyze** | Read existing code, ADRs, and related modules; note constraints. |
| **Plan** | Propose a small, scoped plan (files, behavior, tests). **Share the plan before large changes.** |
| **Implement** | Make minimal, isolated diffs that match the plan. |
| **Test** | Run/add tests with the change; verify happy path and at least one edge case when relevant. |
| **Explain** | Summarize what changed, why, and trade-offs in the **Notion task** (see below) and in the PR/branch if applicable. |
| **Done** | Acceptance criteria met; Notion task updated; learning notes and interview prep when required. |

Do not skip **Analyze** and **Plan** for non-trivial work.

---

## Branches and Notion tasks

Work is tracked in **[AI Knowledge Assistant — Tasks](https://app.notion.com/p/290170951615400186ccceea12cb1dd4)** (Notion). Each unit of work maps to a task id such as `P1-01`, `P5-03`.

### Branch naming

- Open **one branch per Notion task**, named after the task id (lowercase optional but id must be recognizable):
  - Examples: `P1-01`, `p1-01-aspnet-api-foundation`, `P0-02-docker-compose`
- The **current git branch** is the primary hint for which task is in progress; check branch name before implementing.
- Set Notion **Branch / PR** on the task when the branch or PR exists.

### Notion task update (required every task)

When a task’s implementation is finished (or a meaningful milestone is delivered), **always add or update the Notion task page** with a clear description of the work. The user expects this on every task; do not rely only on chat or commit messages.

Include at minimum:

| Section | Content |
|---------|---------|
| **What changed** | Files/areas touched; behavior added or fixed |
| **Why** | Link to acceptance criteria and design intent |
| **How to verify** | Commands, endpoints, or manual steps |
| **Out of scope** | What was intentionally not done |
| **Follow-ups** | Next task, risks, or ADR needed |

Also update when applicable:

- **Status** → **Done** (task bittiğinde zorunlu; sadece In progress bırakma)
- **Branch / PR** → branch name and PR URL
- **Interview Questions** (database property) → task’taki mülakat sorularının cevapları (özet metin)
- **Öğrenme notlarım** / **Mülakat hazırlığı** (sayfa içeriği) → doldurulmuş; boş şablon bırakma
- **Definition of Done** → tüm maddeler işaretli

AI assistants (Cursor): after **Implement** and **Test**, **always** update the Notion task via MCP when available: set Status to Done, fill Interview Questions + page sections (Uygulama özeti, öğrenme notları, mülakat cevapları). Do not rely on chat-only summaries. See `.cursor/rules/notion-task-completion.mdc`.

---

## Cursor / AI assistant rules

1. **Analyze first** — Inspect the current solution, module boundaries, and recent ADRs before editing.
2. **Plan before big changes** — For multi-file or architectural work, produce a short plan and wait for approval when the user expects it.
3. **Show the plan before implementation** when the scope is unclear or crosses modules.
4. **Small, isolated changes** — Prefer one concern per commit/PR; avoid drive-by refactors.
5. **No unnecessary abstraction** — Do not add layers, interfaces, or patterns “for later” without a concrete use case.
6. **No unnecessary dependencies** — Add NuGet/npm packages only when they solve a real problem in scope.
7. **Respect architecture** — Follow the modular monolith layout (`Api`, `Application`, `Domain`, `Infrastructure`) and [ADRs](../adr/README.md). Propose a new ADR before violating an accepted decision.
8. **Tests with implementation** — Update or add unit/integration tests in the same task when behavior changes.
9. **Never commit secrets** — No API keys, passwords, or real `.env` files. Use `appsettings` placeholders, user secrets, and `docker/.env.example`.
10. **Keep docs in sync** — Update README and ADRs when behavior, setup, or architecture decisions change.
11. **Explain on completion** — After a task, document changes on the **Notion task page** (required) and summarize in chat/PR; use the branch’s task id to find the right page.
12. **No large refactors unless asked** — Do not rename/restructure the whole solution without explicit request.
13. **Surface ambiguity early** — Before generating code, call out unclear requirements or decisions that need an ADR or product choice.
14. **Teach through decisions** — For learning goals, briefly explain *why* a technique was chosen (e.g. pgvector vs separate vector DB), not only *what* was coded.

---

## Repository conventions

- **Solution:** `src/AIKnowledgeAssistant.slnx`, projects under `src/` and `tests/`.
- **Modules (Application):** Auth, Documents, Ingestion, Retrieval, Chat — keep feature code grouped; depend inward on `Domain`.
- **Infrastructure:** EF Core, external APIs, messaging, file storage — implement interfaces defined in `Application`.
- **Phases:** P0–P12 roadmap in Notion; do not pull in P7+ tech (RabbitMQ workers, SK, agents) until the task phase requires it.
- **Build policy:** `Directory.Build.props` — nullable enabled, warnings as errors.

---

## When to write an ADR

Create or update an ADR when you:

- Choose or change persistence, messaging, auth, or deployment shape.
- Introduce a new integration (LLM provider, cache, queue).
- Adopt a pattern that affects multiple modules (e.g. async ingestion, SSE contract).

Use [docs/adr/TEMPLATE.md](../adr/TEMPLATE.md) and the numbering rules in [docs/adr/README.md](../adr/README.md).

---

## Definition of Done (task level)

Aligned with the Notion project:

- Code builds; tests pass (or new tests justify absence).
- Acceptance criteria satisfied.
- No secrets in diff.
- README/ADR updated if setup or decisions changed.
- **Notion task** contains an implementation summary (what / why / how to verify / out of scope).
- **Branch / PR** field on the task matches the work branch.
- Learning notes / interview questions on the task when the phase requires them.

---

## Related documents

- [Architecture overview](../architecture/overview.md)
- [ADR index](../adr/README.md)
- [ADR-001: Modular monolith](../adr/ADR-001-modular-monolith.md)
