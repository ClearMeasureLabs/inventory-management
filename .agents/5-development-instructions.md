# Development Agent

## Purpose

Implement the feature or fix according to the technical design, following architecture rules and testing requirements.

## Trigger

- Issue has `Test-Design` label, OR
- Issue has `Technical-Design` label (if test design was skipped)

## Prerequisites

**MUST complete before starting:**

1. **Docker running:** `docker info`
2. **Feature branch created:** `issues/[number]/[title-kebab-case]`
3. **Draft PR exists:** `gh pr create --draft --title "[Title]" --body "Closes #[number]"`
4. **Tools installed:** `.\scripts\install_tools.ps1`

## Inputs

| Input | Source |
|-------|--------|
| Conceptual definition | GitHub issue |
| Technical design | GitHub issue comment |
| UX design | GitHub issue comment (if applicable) |
| Test design | GitHub issue comment (if applicable) |

## Process

1. **Implement code** - Follow technical design steps in order
2. **Add tests** - Unit, integration, Angular, and acceptance tests as specified
3. **Validate** - Run `scripts/build_and_test.ps1` and fix all failures
4. **Commit and push** - Clear commit messages to feature branch

## Architecture Rules

```
Presentation → Infrastructure → Application → Domain
```

**CQRS Structure:**
```
Features/{Entity}/{Action}{Entity}/
├── {Action}{Entity}Command.cs
├── {Action}{Entity}CommandHandler.cs
├── I{Action}{Entity}CommandHandler.cs
└── {Entity}{Action}Event.cs
```

**Handler Responsibilities:**
- Validate input (throw `ValidationException`)
- Create/modify domain entities
- Persist via `IRepository`
- Cache via `ICache`
- Publish via `IEventHub`
- Return DTOs

## Testing Requirements

| Change Type | Required Tests |
|-------------|----------------|
| Backend functionality | Unit + Integration |
| UI components | Angular (`*.spec.ts`) |
| Bug fixes | Regression test |
| User-facing features | Acceptance tests |

**.NET:** Arrange-Act-Assert with `#region` blocks, Bogus for test data

**Angular:** `fakeAsync`/`tick` for async, `HttpTestingController` for API mocks

## Database Migrations

```powershell
.\scripts\add_migration.ps1 -MigrationName <Name>
```

Migrations auto-apply on startup.

## Output

**Labels:** Remove `Test-Design` or `Technical-Design`, Add `Development`

## Rules

1. **ALWAYS** follow the technical design implementation steps in order
2. **ALWAYS** respect layer boundaries - dependencies point inward only
3. **ALWAYS** return DTOs from handlers - NEVER return domain entities
4. **ALWAYS** add all required tests as specified in technical design
5. **NEVER** change NuGet packages, SDK versions, or add `.csproj` projects without approval
6. **NEVER** proceed until `scripts/build_and_test.ps1` passes completely

## Gate

**`build_and_test.ps1` MUST pass before proceeding.**

Expected output:
- Angular build: Success
- Solution build: Success
- Unit tests: Passed
- Integration tests: Passed
- Angular tests: Passed

## Next Phase

**Code Review**
