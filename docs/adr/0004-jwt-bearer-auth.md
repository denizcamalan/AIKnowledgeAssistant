# ADR-0004: JWT bearer authentication

## Status

Accepted (implemented in P2-01)

## Context

The React SPA calls a stateless REST API and SSE endpoints. Session cookies across domains are awkward for local dev; we want a common pattern for APIs and interviews.

## Decision

Use **JWT bearer tokens** issued after login/register. API validates signature, expiry, and claims; document and chat operations are scoped to the authenticated user id.

## Consequences

- **Positive:** Stateless API instances, straightforward SPA integration.
- **Negative:** Revocation and refresh-token strategy must be designed in P2; secrets stay in configuration, never in source control.
