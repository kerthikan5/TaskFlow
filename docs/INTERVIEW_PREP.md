# TaskFlow — Interview Preparation Guide

Use this guide when explaining TaskFlow in technical interviews.

---

## 30-Second Elevator Pitch

> "TaskFlow is a full-stack project management REST API I built from scratch using ASP.NET Core .NET 10, following Clean Architecture. It includes JWT authentication, role-based resource authorization, Entity Framework Core with SQLite, and a React 19 TypeScript frontend. I also wrote unit tests and end-to-end integration tests using WebApplicationFactory. The goal was to demonstrate production-grade engineering patterns — not shortcuts."

---

## Likely Interview Questions & Model Answers

---

### Q: Walk me through the architecture.

**A:** I used Clean Architecture with four layers:

1. **Domain** — Pure C# classes: `User`, `Project`, `TaskItem`, `ProjectMember`. No external dependencies.
2. **Application** — Service interfaces (`IProjectService`, `ITaskService`), DTOs, and custom exceptions. Depends only on Domain.
3. **Infrastructure** — EF Core `AppDbContext`, JWT token generator, `CurrentUserService`. Implements Application interfaces.
4. **API** — ASP.NET Core controllers, global exception-handling middleware, and dependency injection wiring.

The key rule is that **outer layers depend on inner layers, never the reverse**. This keeps the domain and business logic testable in isolation.

---

### Q: How did you implement authentication?

**A:** I used JWT Bearer tokens:

- On register/login, I hash the password using ASP.NET Core's `PasswordHasher<User>` (which uses PBKDF2) and generate a signed JWT.
- The JWT contains claims: `sub` (userId), `email`, `role`, and standard `iat`/`exp`.
- I created `ICurrentUserService` which reads the `sub` claim from `HttpContext.User` to identify the caller in any service without injecting the controller or HTTP context directly.
- A global `ExceptionHandlingMiddleware` maps domain exceptions (`ForbiddenException`, `NotFoundException`) to RFC-7807 `ProblemDetails` HTTP responses.

---

### Q: How does authorization work?

**A:** I implemented resource-based authorization manually in service methods:

- **Project read access**: User must be the owner (`OwnerId == currentUserId`) OR appear in `ProjectMembers`.
- **Project mutations** (update/delete): Only the `OwnerId`.
- **Member removal**: The owner can remove anyone; a member can remove themselves; no one can remove the owner.
- **Task creation**: User must be an owner or member of the project.
- **Assignee validation**: The assignee must also be an owner or member of the project — you can't assign a task to someone outside the team.

---

### Q: How did you handle database relationships?

**A:** I used EF Core with Fluent API configuration files per entity:

- `User` → `Project` (one-to-many via `OwnerId`)
- `Project` → `ProjectMember` (one-to-many join table with composite key `[ProjectId, UserId]`)
- `Project` → `TaskItem` (one-to-many)
- `TaskItem` → `User` (two FKs: `CreatedById` and `AssigneeId`, configured with `DeleteBehavior.Restrict` to prevent cascade loops)

---

### Q: How did you test the project?

**A:** I wrote two levels of tests:

1. **Unit Tests** (`TaskFlow.UnitTests`): Tested `JwtTokenGenerator` by issuing a token and then validating it using `JwtSecurityTokenHandler` — verifying claims, signature, issuer, and audience.

2. **Integration Tests** (`TaskFlow.IntegrationTests`): Used `WebApplicationFactory<Program>` to spin up the real ASP.NET Core pipeline against an isolated SQLite database. Each test class gets its own uniquely named `.db` file, deleted after disposal. I tested a full user story: register → login → create project → invite member → create task → update status.

---

### Q: How would you scale this for production?

**A:** Several improvements for production:

| Area | Change |
|------|--------|
| Database | Switch from SQLite to PostgreSQL (one connection string change) |
| Auth | Add token refresh endpoint and token blacklist (Redis) |
| Caching | Add Redis caching for project/member lookups |
| Validation | Add FluentValidation for richer input checks |
| Logging | Structured logging with Serilog → Seq or Datadog |
| CI/CD | GitHub Actions pipeline running `dotnet test` on every PR |
| Containerization | Dockerfile + docker-compose for API + DB |

---

### Q: What would you do differently?

**A:**
1. **Add FluentValidation** instead of inline `throw new ValidationException(...)` for cleaner, centralized validation with reusable rules.
2. **Use MediatR** (CQRS pattern) to split read and write operations, making the Application layer even more explicit.
3. **Token Refresh** — the current JWT has no refresh mechanism.
4. **Audit Logging** — track who changed what and when for compliance.
5. **Rate Limiting** — protect the auth endpoints against brute-force.

---

## Key Numbers to Remember

- **4 layers**: Domain, Application, Infrastructure, API
- **3 test classes**: JwtTokenGeneratorTests, AuthIntegrationTests, ProjectTaskIntegrationTests
- **17 REST endpoints** across 5 controllers
- **5 custom exception types** → RFC-7807 ProblemDetails
- **0 direct entity exposure** in API responses (always mapped to DTOs)

---

## GitHub Talking Points

When the interviewer looks at your GitHub repo, point out:

1. **Commit history** — conventional commits (`feat:`, `fix:`, `test:`)
2. **Clean Architecture** — clearly separated projects with enforced dependency rules
3. **`docs/ARCHITECTURE_DECISIONS.md`** — shows you think about *why* not just *how*
4. **Integration tests** — not just unit tests
5. **`.gitignore`** — `taskflow.db`, secrets, and build artifacts excluded
6. **Clean `Program.cs`** — minimal, readable bootstrapping using extension methods
