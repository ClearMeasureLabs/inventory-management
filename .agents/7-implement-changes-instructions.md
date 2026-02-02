# Implement Changes Agent

## Purpose

Implement requested changes from code review feedback, fix issues, and re-submit the merge request.

## Trigger

- Merge request converted to draft (automated via `.github/workflows/code-review-changes-cursor.yml`)
- Merge request has review comments with requested changes

## Inputs

| Input | Source |
|-------|--------|
| Review comments | Merge request |
| Linked work item | Merge request description (linked to and completes work item #N) |
| Original requirements | Work item |
| Technical design | Work item comment |

## Process

1. **Post starting comment:** "Starting automated implementation of requested changes..."
2. **Fetch linked work item** - Read requirements and technical design
3. **Read review comments** - Extract files, line numbers, and requested changes
4. **Implement each change** - Locate code, apply fix, verify it works
5. **Run validation:** `.\scripts\build_and_test.ps1`
6. **Commit and push** - Single commit with summary of all changes
7. **Post summary comment**
8. **Mark merge request ready for review** - Use platform CLI to trigger Code Review agent

## Output Templates

### Starting Comment
```markdown
Starting automated implementation of requested changes...
```

### Commit Message
```
Address review feedback: [summary]

- [Change 1]
- [Change 2]
- [Change 3]
```

### Summary Comment
```markdown
## Changes Implemented

### Completed
- [x] [Change with file:line reference]
- [x] [Change with file:line reference]

### Verification
- [x] `scripts/build_and_test.ps1` passing
- [x] No new linter errors

### Manual Attention Required
[Items that could not be resolved] OR "None"
```

### Unresolvable Issue Format
```markdown
### Unable to Resolve

**Requested:** [What was requested]
**Issue:** [Why it could not be implemented]
**Suggested Manual Steps:**
1. [Step]
2. [Step]

**Status:** Merge request remains in draft for manual attention.
```

## Rules

1. **ALWAYS** post starting comment before beginning work
2. **ALWAYS** follow architecture and testing rules when implementing fixes
3. **ALWAYS** run `build_and_test.ps1` before pushing
4. **ALWAYS** use a single commit for all changes in one implementation cycle
5. **ALWAYS** document unresolvable issues with clear explanation and suggested manual steps
6. **NEVER** mark merge request ready if significant issues remain unresolved
7. **NEVER** introduce new issues - fixes MUST NOT break other functionality

## Flow

```
Review Requests Changes
        │
        ▼
   Merge Request → Draft (triggers this agent)
        │
        ▼
   Implement Changes
        │
        ▼
   build_and_test.ps1
        │
        ├──► Pass ──► Commit, Push, Mark Ready
        │
        └──► Fail ──► Fix, Retry
                         │
                         └──► Unresolvable ──► Document, Keep Draft
```

## Next Phase

Returns to **Code Review Agent** for re-evaluation.

Cycle continues until:
- Merge request approved (all checks pass), OR
- Manual intervention required (unresolvable issues documented)
