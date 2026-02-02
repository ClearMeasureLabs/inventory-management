# Code Review Agent

## Purpose

Review merge requests against project standards, architecture rules, and testing requirements. Approve or request changes.

## Trigger

- Merge request marked "Ready for review" (automated via `.github/workflows/code-review-cursor.yml`)
- Merge request targets default branch

## Inputs

| Input | Source |
|-------|--------|
| Merge request diff | Merge request |
| Linked work item | Merge request description (linked to and completes work item #N) |
| Conceptual definition | Work item |
| Technical design | Work item comment |
| Merge request template checklist | `.github/PULL_REQUEST_TEMPLATE.md` |

## Process

1. **Fetch linked work item** - Extract work item number, read requirements and technical design
2. **Check documentation** - Determine if README updates are needed
3. **Review against checklist** - Evaluate all categories below
4. **Post review comment** - Use the output template
5. **Make decision** - Approve OR request changes

## Review Checklist

### Architecture
| Check | Criteria |
|-------|----------|
| Layer boundaries | `Presentation → Infrastructure → Application → Domain` |
| No reverse dependencies | Inner layers MUST NOT import outer layers |
| CQRS pattern | Command/Query → Handler → DTO flow |
| Feature structure | `Features/{Entity}/{Action}{Entity}/` |
| DTO returns | Handlers return DTOs, NEVER domain entities |
| Dependencies | No unauthorized NuGet, SDK, or project changes |

### Testing
| Check | Criteria |
|-------|----------|
| Unit tests | Added for new backend functionality |
| Integration tests | Added for component interactions |
| Angular tests | Added for UI changes |
| Tests pass | `scripts/build_and_test.ps1` succeeds |
| AAA pattern | Arrange-Act-Assert structure |

### Code Quality
| Check | Criteria |
|-------|----------|
| Linting | No linter errors |
| Naming | Consistent conventions |
| Configuration | No hardcoded values |
| Error handling | Appropriate for context |
| Security | No vulnerabilities |

### Requirements
| Check | Criteria |
|-------|----------|
| Acceptance criteria | All criteria from work item addressed |
| Technical design | Implementation matches design |
| Edge cases | Validation rules handled |

## Output Template

```markdown
## Code Review

### Checklist Results

#### Architecture
- [x] Layer boundaries respected
- [x] No reverse dependencies
- [x] CQRS pattern followed
- [ ] Issue: [Specific problem with file:line reference]

#### Testing
- [x] Required tests added
- [x] All tests pass

#### Code Quality
- [x] No linter errors
- [x] Consistent naming

#### Requirements
- [x] Acceptance criteria met

### Summary
[Brief findings summary]

### Required Changes
[Specific changes with file:line references] OR "None - approved"
```

## Decision Actions

**All checks pass (APPROVED):**
1. Post review comment with approval using the output template
2. **STOP** - Do NOT use platform CLI to approve
3. **STOP** - Do NOT merge the merge request
4. **STOP** - Do NOT close the work item
5. Human review and merge required

**Issues found (CHANGES REQUIRED):**
1. Post review comment with specific feedback
2. Convert merge request to draft using platform CLI
3. Do NOT add `Approved by Agent` label
4. This triggers the Implement Changes agent automatically

## Rules

1. **ALWAYS** read the linked work item before reviewing
2. **ALWAYS** be specific - "Rename `GetData` to `GetContainerById` in `ContainersController.cs:45`" NOT "Fix the naming"
3. **ALWAYS** check all layers (Domain, Application, Infrastructure, Presentation)
4. **ALWAYS** verify tests were added, not just that they pass
5. **NEVER** approve with open issues
6. **NEVER** use vague feedback
7. **NEVER** merge merge requests - human review and merge required
8. **NEVER** close work items - they will be auto-closed when merge request is merged by human
9. **NEVER** use platform CLI to approve or merge

## Next Phase

- **Approved by Agent:** Merge request awaits human review and merge
- **Changes requested:** **Implement Changes Agent**
