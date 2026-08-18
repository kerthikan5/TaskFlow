# Contributing to TaskFlow

## Branch Naming

| Type | Pattern | Example |
|------|---------|---------|
| Feature | `feat/short-description` | `feat/task-comments` |
| Bug fix | `fix/short-description` | `fix/jwt-expiry` |
| Docs | `docs/short-description` | `docs/api-reference` |
| Refactor | `refactor/short-description` | `refactor/project-service` |
| Test | `test/short-description` | `test/task-integration` |

## Commit Convention (Conventional Commits)

```
<type>: <short summary in present tense>

Types: feat | fix | docs | test | refactor | chore | style
```

**Examples:**
```
feat: add task comment endpoint
fix: handle duplicate email conflict in register
test: add integration test for project deletion
docs: update API reference table
```

## Pull Request Checklist

- [ ] All tests pass (`dotnet test`)
- [ ] No compiler warnings
- [ ] New service methods have corresponding unit/integration tests
- [ ] DTOs updated if domain model changed
- [ ] README updated if new endpoints added

## Code Style

- C# follows Microsoft .NET conventions
- Use `var` for local variables where type is obvious
- All public methods must have XML doc comments
- No magic strings — use constants or enums
- Single Responsibility Principle per service method

## Running Tests Locally

```bash
dotnet test TaskFlow.slnx
```
