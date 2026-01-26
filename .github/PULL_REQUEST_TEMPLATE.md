## Summary

<!-- Briefly describe the changes in this PR -->

## Related Issue

Closes #<!-- issue number -->

## Changes Made

<!-- List the key changes made in this PR -->

- 

---

## Code Review Checklist

The following checklist will be used by the automated Cursor Cloud Agent reviewer. Ensure your PR addresses these items before marking it ready for review.

### Architecture Rules

- [ ] Layer boundaries respected (Presentation → Infrastructure → Application → Domain)
- [ ] No reverse dependencies (inner layers do not depend on outer layers)
- [ ] CQRS pattern followed for new features (Command/Query → Handler → DTO)
- [ ] Feature organization follows `Features/{Entity}/{Action}{Entity}/` structure
- [ ] Handlers return DTOs, never domain entities
- [ ] No unauthorized changes to NuGet packages, SDK versions, or new projects

### Automated Testing

- [ ] Unit tests added/updated for new backend functionality
- [ ] Integration tests added/updated for component interactions
- [ ] Angular component tests added/updated for UI changes
- [ ] All existing tests still pass (`scripts/build_and_test.ps1`)
- [ ] Tests follow Arrange-Act-Assert pattern
- [ ] Test coverage includes: dependencies called, business logic, state changes, return values, exceptions

### Documentation

- [ ] README updated if new features, scripts, or configuration added
- [ ] Code comments added for complex logic
- [ ] API contracts documented if endpoints changed

### Code Quality

- [ ] No linter errors introduced
- [ ] Consistent naming conventions followed
- [ ] No hardcoded values that should be configuration
- [ ] Error handling implemented appropriately
- [ ] No security vulnerabilities introduced

### Issue Requirements

- [ ] All acceptance criteria from the linked issue are addressed
- [ ] Implementation matches the technical design (if documented)
- [ ] Edge cases and validation rules from the issue are handled

---

## Reviewer Notes

<!-- Optional: Add any notes for reviewers, areas of concern, or specific feedback requested -->
