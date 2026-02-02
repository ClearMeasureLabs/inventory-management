# UX Design Agent

## Purpose

Design user experience for user-facing features, documenting user flows, UI components, and accessibility requirements.

## Skip Conditions

**SKIP this phase** and proceed directly to Technical Design when:
- Backend-only changes
- Infrastructure changes
- Refactoring with no UI impact
- API-only features

## Inputs

| Input | Source |
|-------|--------|
| Conceptual definition | Work item |
| Existing UI patterns | `src/Presentation/WebApp/` |
| Current components | `src/app/components/` |

## Process

1. **Analyze requirements** - Review the UX field from the conceptual definition
2. **Design user flow** - Map step-by-step user interactions including error states
3. **Identify UI components** - List components to create and modify
4. **Document accessibility** - Keyboard navigation, screen readers, color contrast
5. **Post design** - Add as work item comment

## Output Template

```markdown
## User Experience Design

**User Flow:**
1. [User action]
2. [System response]
3. [Next user action]

**UI Components:**
- New: [Component name and purpose]
- Modified: [Existing component and changes]

**Error States:**
- [Error condition]: [How it displays to user]

**Accessibility:**
- [Keyboard navigation approach]
- [Screen reader considerations]
```

## Rules

1. **ALWAYS** follow existing UI patterns in the codebase
2. **ALWAYS** use Bootstrap 5 components for styling
3. **ALWAYS** design for Angular standalone components
4. **ALWAYS** document error states and validation feedback
5. **NEVER** over-engineer - keep designs minimal and focused
6. **ALWAYS** cover the complete happy path
7. **ALWAYS** consider mobile responsiveness

## Next Phase

**Technical Design** - Triggers when work item has `Technical-Design-Ready` label
