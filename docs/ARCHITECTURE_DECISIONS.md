# Architecture Decision Records

This document records the key technology and design decisions made during TaskFlow development, along with rationale.

---

## ADR-001: Clean Architecture Layer Separation

**Decision:** Enforce strict Clean Architecture with four layers: Domain, Application, Infrastructure, and API.

**Rationale:**
- Domain and Application layers have zero infrastructure dependencies — they are testable in isolation.
- Swapping the database (e.g., SQLite → PostgreSQL) requires changes only in the Infrastructure layer.
- Services are defined as interfaces in Application and implemented in Infrastructure, enabling dependency injection and Moq-based unit testing.

**Consequence:** More initial boilerplate, but significantly easier to maintain and scale.

---

## ADR-002: SQLite for Local Development

**Decision:** Use SQLite as the database engine instead of PostgreSQL/SQL Server.

**Rationale:**
- Zero configuration — no Docker, no local DB server installation required.
- EF Core supports SQLite natively with full migration support.
- SQLite is sufficient for demonstrating all relational patterns used in this project.
- Switching to PostgreSQL for production requires only a single connection string change and package swap.

**Consequence:** SQLite file (`taskflow.db`) is gitignored. No shared state in CI.

---

## ADR-003: JWT Bearer Authentication

**Decision:** Use stateless JWT Bearer tokens for authentication.

**Rationale:**
- Stateless auth scales horizontally without session stores.
- JWT is the industry standard for REST API auth.
- ASP.NET Core has first-class JWT middleware support.
- Claims-based identity allows `ICurrentUserService` to extract userId from HTTP context in any service.

**Consequence:** Token revocation requires short expiry or a token blacklist (not implemented — out of scope for portfolio).

---

## ADR-004: RFC-7807 ProblemDetails Error Responses

**Decision:** Map all domain exceptions to RFC-7807 `ProblemDetails` format via global middleware.

**Rationale:**
- Consistent error format across all endpoints.
- Clients parse a predictable structure: `{ title, status, detail }`.
- Middleware catches exceptions so controllers remain clean (no try/catch).

**Exception → Status code mapping:**

| Exception | HTTP Status |
|-----------|------------|
| `ValidationException` | 400 Bad Request |
| `NotFoundException` | 404 Not Found |
| `ConflictException` | 409 Conflict |
| `ForbiddenException` | 403 Forbidden |
| Unhandled | 500 Internal Server Error |

---

## ADR-005: DTO Separation from Domain Entities

**Decision:** Never expose domain entities directly from API. Always use request/response DTOs.

**Rationale:**
- Prevents circular JSON serialization (navigation property loops).
- Prevents accidental sensitive field exposure (e.g., password hashes).
- Allows independent evolution of API contract and domain model.
- Cleaner Swagger documentation.

---

## ADR-006: Resource-Based Authorization (Manual)

**Decision:** Implement resource-based authorization in service methods rather than using `IAuthorizationHandler`.

**Rationale:**
- Simpler to understand and debug for a portfolio project.
- Helper method `GetCurrentUserIdOrThrow()` in every service centralizes the auth check.
- Project ownership checks are co-located with business logic.

**Future:** For complex RBAC scenarios, migrate to `IAuthorizationHandler` with `AuthorizationPolicy`.

---

## ADR-007: WebApplicationFactory for Integration Tests

**Decision:** Use `Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>` for integration tests.

**Rationale:**
- Tests the full HTTP pipeline: routing, middleware, auth, serialization.
- Each test run uses a uniquely named SQLite file — zero shared state.
- Database file is deleted in `Dispose()` for clean teardowns.
- `public partial class Program {}` in `Program.cs` exposes the entry point to the test project.
