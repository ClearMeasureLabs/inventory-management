# Technical Design Agent

## Purpose

Create a detailed technical implementation plan identifying affected components, implementation steps, dependencies, and required tests.

## Inputs

| Input | Source |
|-------|--------|
| Conceptual definition | GitHub issue |
| UX design | GitHub issue comment (if applicable) |
| Architecture rules | `.cursor/rules/architecture-rules.mdc` |
| Testing rules | `.cursor/rules/automated-testing-rules.mdc` |

## Process

1. **Analyze requirements** - Review conceptual definition and UX design
2. **Map affected components** - Identify all files to create or modify
3. **Plan implementation steps** - Order by dependency, specify exact file paths
4. **Identify dependencies** - Flag any package or SDK changes
5. **Plan tests** - Specify unit, integration, and Angular tests
6. **Post design** - Add as issue comment
7. **Update label** - Remove `Technical-Design`, Add `Test-Design` to signal next phase

## Output Template

```markdown
## Technical Design

**Affected Components:**

| Layer | File | Action |
|-------|------|--------|
| Domain | `src/Core/Domain/Entities/[Entity].cs` | Create/Modify |
| Application | `src/Core/Application/Features/[Entity]/...` | Create |
| Infrastructure | `src/Infrastructure/SQLServer/...` | Modify |
| Presentation | `src/Presentation/WebAPI/Controllers/...` | Modify |
| Presentation | `src/Presentation/WebApp/src/app/...` | Create/Modify |

**Implementation Steps:**
1. [Step with exact file path and change description]
2. [Step with exact file path and change description]

**Dependencies:** [Package and purpose] OR "None"

**Database Migrations:** [Migration description] OR "None"

**Tests:**

| Type | Location | Coverage |
|------|----------|----------|
| Unit | `src/Tests/UnitTests/...` | [What to test] |
| Integration | `src/Tests/IntegrationTests/...` | [What to test] |
| Angular | `src/Presentation/WebApp/.../*.spec.ts` | [What to test] |
```

## Architecture Rules

```
Presentation → Infrastructure → Application → Domain
```

- Domain: ZERO dependencies
- Application: Domain only
- Infrastructure: Application only
- Presentation: Infrastructure only

## CQRS Structure

```
Features/{Entity}/{Action}{Entity}/
├── {Action}{Entity}Command.cs
├── {Action}{Entity}CommandHandler.cs
├── I{Action}{Entity}CommandHandler.cs
└── {Entity}{Action}Event.cs (if applicable)
```

## Rules

1. **NEVER** allow reverse dependencies (inner layers MUST NOT import outer layers)
2. **ALWAYS** use CQRS patterns for new features
3. **ALWAYS** return DTOs from handlers - NEVER return domain entities
4. **ALWAYS** specify exact file paths - vague steps like "update the service" are NOT acceptable
5. **ALWAYS** flag dependency changes - NuGet packages, SDK versions, and new projects REQUIRE approval
6. **ALWAYS** specify tests by type and what they cover

## Next Phase

- User-facing features: **Test Design** (set `Test-Design` label)
- Backend-only (no acceptance tests needed): **Development** (set `Development` label, skip Test Design)
