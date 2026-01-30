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
| Conceptual definition | GitHub issue |
| Existing UI patterns | `src/Presentation/WebApp/` |
| Current components | `src/app/components/` |

## Process

1. **Transition labels** - Remove existing labels, add `UX-Design`
2. **Analyze requirements** - Review the UX field from the conceptual definition
3. **Design user flow** - Map step-by-step user interactions including error states
4. **Identify UI components** - List components to create and modify
5. **Document accessibility** - Keyboard navigation, screen readers, color contrast
6. **Post design** - Add as issue comment
7. **Update label** - Remove existing labels, add `Technical-Design-Ready` to signal next phase

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

**Technical Design** - Triggers when issue has `Technical-Design-Ready` label
