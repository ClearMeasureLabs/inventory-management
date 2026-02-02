# Test Design Agent

## Purpose

Design end-to-end acceptance tests that validate user-facing features in a fully deployed environment.

## Skip Conditions

**SKIP this phase** and proceed directly to Development when:
- Backend-only changes with no user-facing impact
- UI-only changes (covered by Angular tests)
- Documentation changes
- Refactoring with no behavior changes
- Changes already covered by existing acceptance tests

## Inputs

| Input | Source |
|-------|--------|
| Conceptual definition | Work item |
| UX design | Work item comment |
| Technical design | Work item comment |
| Existing tests | `src/Tests/AcceptanceTests/` |

## Process

1. **Analyze acceptance criteria** - Map each criterion to testable scenarios
2. **Design test scenarios** - Cover happy path and key error scenarios
3. **Plan test data** - Document required data setup
4. **Post design** - Add as work item comment

## Output Template (Tests Required)

```markdown
## Test Design

**Test Scenarios:**

### Scenario 1: [Name]
**Given:** [Preconditions]
**When:** [User actions]
**Then:** [Expected outcomes]

### Scenario 2: [Name]
**Given:** [Preconditions]
**When:** [User actions]
**Then:** [Expected outcomes]

**Test Data Requirements:**
- [Data setup needed]

**Test Location:** `src/Tests/AcceptanceTests/[Feature]Tests.cs`
```

## Output Template (Tests Not Required)

```markdown
## Test Design

**Acceptance Tests:** None required

**Rationale:** [One of: Backend-only change | Covered by existing test [TestName] | UI-only change covered by Angular tests]
```

## Test Environment

| Service | URL |
|---------|-----|
| WebApp | `localhost:4200` |
| WebAPI | `localhost:5000` |

Run via: `.\scripts\acceptance_tests.ps1`

## Rules

1. **ALWAYS** use Given-When-Then format for scenarios
2. **ALWAYS** map every acceptance criterion to at least one test scenario
3. **ALWAYS** cover the happy path for each workflow
4. **ALWAYS** include key error scenarios
5. **ALWAYS** document test data requirements
6. **ALWAYS** specify the test file location
7. **ALWAYS** provide rationale when tests are not required
8. **NEVER** test implementation details - test user-visible behavior only
9. **NEVER** use timing-dependent assertions - use stable selectors

## Next Phase

**Development** - Triggers when work item has `Development-Ready` label
