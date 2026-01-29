# Software Development Lifecycle

This guide describes how to develop features and fixes in this repository.

## Workflow Overview

```
Issue → Design → Develop → Test → Review → Merge
```

Every code change starts with a GitHub issue. The process is:

1. Create an issue with a clear definition of what you're building
2. Design the solution (UX and technical)
3. Implement and test
4. Submit for review
5. Address feedback and merge

## Creating an Issue

Before writing code, create a GitHub issue with a **Conceptual Definition** that answers:

| Question | Example |
|----------|---------|
| What does this accomplish? | "Allow users to export inventory reports as CSV" |
| How will users interact with it? | "Export button on the reports page, downloads immediately" |
| Who can access it? | "Any authenticated user" or "Admins only" |
| What validation is needed? | "Date range required, max 1 year span" |
| How do we know it's done? | Checklist of specific, testable criteria |

### Issue Template

```markdown
# Conceptual Definition

**Goal:** Allow users to export inventory reports as CSV files

**UX:** Export button on reports page triggers immediate download

**Authorization:** Any authenticated user

**Validation:**
- Date range is required
- Maximum span of 1 year
- At least one column must be selected

**Acceptance Criteria:**
- [ ] Export button visible on reports page
- [ ] CSV downloads with correct headers
- [ ] Date validation shows error for invalid ranges
- [ ] Empty reports show appropriate message
```

Label the issue `Conceptual-Definition` when created.

## Design Phase

### UX Design

For user-facing features, document the user experience before coding:

- **User flow** - Step-by-step interaction
- **UI components** - What's new or changing
- **Error states** - What happens when things go wrong
- **Accessibility** - Keyboard navigation, screen readers

Post as an issue comment and update the label to `UX-Design`.

Skip this for backend-only work.

### Technical Design

Plan your implementation:

- **Affected files** - What you'll create or modify
- **Implementation steps** - Ordered list of changes
- **Dependencies** - New packages (requires approval)
- **Tests** - What tests you'll write

Post as an issue comment and update the label to `Technical-Design`.

### Test Design

For user-facing features, plan your acceptance tests using Given-When-Then format:

```markdown
### Scenario: Export with valid date range
**Given:** User is on reports page with data available
**When:** User selects last 30 days and clicks Export
**Then:** CSV file downloads with correct data
```

Post as an issue comment and update the label to `Test-Design`.

Skip this for backend-only work or changes covered by existing tests.

## Development

### Setup

Before starting:

```powershell
# Verify Docker is running
docker info

# Start local infrastructure (SQL Server, Redis, RabbitMQ)
cd environments\local
.\provision.ps1

# Install development tools
.\scripts\install_tools.ps1
```

### Branch and PR

```powershell
# Create feature branch
git checkout -b issues/123/export-csv-reports

# Create draft PR early
gh pr create --draft --title "Add CSV export to reports" --body "Closes #123"
```

### Implementation

Follow your technical design. Key rules:

- **Architecture**: Dependencies flow inward only (`Presentation → Infrastructure → Application → Domain`)
- **CQRS**: New features use Command/Query → Handler → DTO pattern
- **No unauthorized changes**: NuGet packages, SDK versions, and new projects require approval

Update the issue label to `Development`.

### Testing

Run the full test suite before submitting:

```powershell
.\scripts\build_and_test.ps1
```

This must pass before proceeding. It runs:
- Angular build
- .NET build
- Unit tests
- Integration tests
- Angular tests

#### Test Requirements

| Change Type | Required Tests |
|-------------|----------------|
| Backend logic | Unit + Integration tests |
| UI components | Angular tests (`*.spec.ts`) |
| Bug fixes | Regression test |
| User-facing features | Acceptance tests |

## Functional Validation

For features with acceptance tests, run them against the full deployed stack:

```powershell
.\scripts\acceptance_tests.ps1
```

This deploys the entire application via Docker and runs Playwright tests.

Update the issue label to `Functional-Validation`.

## Code Review

When ready for review:

```powershell
gh pr ready
```

Update the issue label to `Ready-For-Review`.

### What Reviewers Check

- **Architecture** - Layer boundaries, CQRS patterns, DTO returns
- **Tests** - Required tests added and passing
- **Code quality** - Naming, error handling, no hardcoded config
- **Requirements** - Acceptance criteria met, edge cases handled

### Review Outcomes

**Approved** - Ready to merge

**Changes Requested** - Address the feedback:
1. Read the review comments carefully
2. Make the requested changes
3. Run `build_and_test.ps1` again
4. Push and mark ready for re-review

## Quick Reference

### Commands

```powershell
# Local infrastructure
cd environments\local && .\provision.ps1

# Build and test (required before PR)
.\scripts\build_and_test.ps1

# Acceptance tests
.\scripts\acceptance_tests.ps1

# Add database migration
.\scripts\add_migration.ps1 -MigrationName AddExportHistory

# Create draft PR
gh pr create --draft --title "[Title]" --body "Closes #123"

# Mark PR ready for review
gh pr ready
```

### Branch Naming

```
issues/[number]/[short-description]
```

Examples:
- `issues/123/export-csv-reports`
- `issues/456/fix-login-redirect`

### Label Progression

```
Conceptual-Definition → UX-Design → Technical-Design → Test-Design → Development → Functional-Validation → Ready-For-Review
```

Skip `UX-Design` for backend-only work.
Skip `Test-Design` when acceptance tests aren't needed.

### Local Services

| Service | URL |
|---------|-----|
| Angular app | http://localhost:4200 |
| API | http://localhost:5000 |
| SQL Server | localhost:1433 |
| Redis | localhost:6379 |
| RabbitMQ | localhost:5672 (management: 15672) |
