# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Issue Requirement

**MANDATORY**: Before making any code changes to this repository, there MUST be an associated GitHub issue. If no issue exists:
1. Ask the user for the issue number
2. If no issue exists yet, create one using the workflow in `.agents/1-issue-creation-instructions.md`

Do NOT proceed with code modifications until an issue is confirmed or created.

## Agent Workflows

When working on features or changes, follow the staged workflow defined in `.agents/`. Read the relevant instruction file before starting each phase:

| Phase | Instructions | Prompt |
|-------|--------------|--------|
| 1. Issue Creation | `.agents/1-issue-creation-instructions.md` | `.agents/1-issue-creation-prompt.txt` |
| 2. UX Design | `.agents/2-ux-design-instructions.md` | `.agents/2-ux-design-prompt.txt` |
| 3. Technical Design | `.agents/3-technical-design-instructions.md` | `.agents/3-technical-design-prompt.txt` |
| 4. Test Design | `.agents/4-test-design-instructions.md` | `.agents/4-test-design-prompt.txt` |
| 5. Development | `.agents/5-development-instructions.md` | `.agents/5-development-prompt.txt` |
| 6. Code Review | `.agents/6-code-review-instructions.md` | `.agents/6-code-review-prompt.txt` |
| 7. Implement Changes | `.agents/7-implement-changes-instructions.md` | `.agents/7-implement-changes-prompt.txt` |
| 8. Functional Validation | `.agents/8-functional-validation-instructions.md` | `.agents/8-functional-validation-prompt.txt` |

Not all phases are required for every change—use judgment based on the scope of work.

## Build and Test Commands

```powershell
# Full build and test (Angular build + .NET build + all tests)
.\scripts\build_and_test.ps1

# Run specific test suites
dotnet test src/Tests/UnitTests
dotnet test src/Tests/IntegrationTests

# Angular tests only
cd src/Presentation/webapp
npm test -- --watch=false --browsers=ChromeHeadless

# Acceptance tests (requires Docker, deploys full stack)
.\scripts\acceptance_tests.ps1

# Add EF Core migration
.\scripts\add_migration.ps1 -MigrationName <name>

# Run the API
dotnet run --project src/Presentation/WebAPI

# Run Angular dev server
cd src/Presentation/webapp && npm start
```

## Architecture

Clean/onion architecture with CQRS. Dependencies point inward only:

```
Presentation → Infrastructure → Application → Domain
```

| Layer | Location | Contains |
|-------|----------|----------|
| Domain | `src/Core/Domain/` | Entities (no dependencies) |
| Application | `src/Core/Application/` | Features, DTOs, interfaces |
| Infrastructure | `src/Infrastructure/` | SQLServer, Redis, RabbitMQ, Bootstrap |
| Presentation | `src/Presentation/` | WebAPI (ASP.NET), WebApp (Angular 19) |

### CQRS Feature Organization

```
Application/Features/{Entity}/
├── {Entity}s.cs                         # Facade
├── I{Entity}s.cs                        # Facade interface
└── {Action}{Entity}/
    ├── {Action}{Entity}Command.cs       # Request
    ├── {Action}{Entity}CommandHandler.cs
    ├── I{Action}{Entity}CommandHandler.cs
    └── {Entity}{Action}Event.cs         # Domain event (if applicable)
```

Request flow: `API Request → Command/Query → Handler → DTO → API Response`

Handlers: validate input (throw `ValidationException`), create/modify entities, persist via `IRepository`, cache via `ICache`, publish via `IEventHub`, return DTOs (never domain entities).

## Testing Requirements

All changes require passing tests via `scripts/build_and_test.ps1`.

| Change Type | Required Tests |
|-------------|----------------|
| Backend functionality | Unit + Integration |
| UI components | Angular (*.spec.ts alongside components) |
| Bug fixes | Regression test at appropriate level |
| User-facing features | Acceptance tests (Playwright) |

.NET tests use Arrange-Act-Assert pattern with `#region` blocks and Bogus for test data. Angular tests use `fakeAsync`/`tick` for async operations.

## Approval Required Before Changing

- NuGet packages
- SDK/framework versions
- New `.csproj` projects

## Prerequisites

Docker must be running before making code changes. Verify with `docker info`.

## Local Infrastructure

Start with `environments\local\provision.ps1`:
- SQL Server: `localhost:1433`
- RabbitMQ: `localhost:5672` (management: `localhost:15672`)
- Redis: `localhost:6379` (insight: `localhost:5540`)
