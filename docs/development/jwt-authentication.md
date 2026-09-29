# JWT authentication (P2-01)

## Flow

1. `POST /api/auth/login` with `{ "email", "password" }`.
2. API validates credentials against the **demo in-memory user store** (`Infrastructure/Auth/DemoUserCredentialStore`).
3. On success, returns a bearer access token (HS256) with `sub`, `email`, `display_name`, and `role` claims.
4. Call protected routes with `Authorization: Bearer <token>` — e.g. `GET /api/account/me`.

## Demo users (development only)

| Email | Password | Roles |
|-------|----------|-------|
| `admin@demo.local` | `Admin123!` | Admin, User |
| `user@demo.local` | `User123!` | User |

Passwords are hashed with `PasswordHasher<T>` at startup; plain text is not stored.

## Configuration

`appsettings.Development.json` sets a dev signing key. Production must set `Jwt__SigningKey` (≥ 32 characters) via environment or user secrets — never commit real secrets.

## Browser client (P2-03)

`client/web` signs in with `POST /api/auth/login`, stores the access token in `localStorage`, and calls protected `GET /api/account/me` with `Authorization: Bearer`. Risks of that storage choice and production alternatives are in the README section **Frontend token storage (P2-03)**.

In local dev, Vite proxies `/api` to `http://localhost:5149`. Direct calls from `http://localhost:5173` are allowed by `Cors:Origins`.

## Authentication vs authorization

- **Authentication** (`UseAuthentication`, login): who is the caller?
- **Authorization** (`[Authorize]`, roles): what may they do? Document CRUD stays anonymous until a later task scopes by user.

## Related

- [ADR-0004: JWT bearer auth](../adr/0004-jwt-bearer-auth.md)
