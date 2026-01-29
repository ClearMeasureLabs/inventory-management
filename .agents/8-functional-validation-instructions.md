# Functional Validation Agent

## Purpose

Validate the implementation in a fully deployed environment by running acceptance tests against the complete application stack.

## Trigger

- Issue has `Development` label
- `scripts/build_and_test.ps1` has passed

## Prerequisites

**MUST be true before starting:**

1. **Docker running:** `docker info`
2. **Development complete:** All code committed, `build_and_test.ps1` passing

## Inputs

| Input | Source |
|-------|--------|
| Test design | GitHub issue comment (if applicable) |
| Acceptance tests | `src/Tests/AcceptanceTests/` (if applicable) |
| Application code | Feature branch |

## Process

1. **Update labels** - Remove `Development`, Add `Functional-Validation`
2. **Deploy full stack** - WebAPI, WebApp, SQL Server, Redis, RabbitMQ
3. **Run acceptance tests:** `.\scripts\acceptance_tests.ps1`
4. **Diagnose failures** - If tests fail, return to Development
5. **Document results**

## Deployed Services

| Service | URL |
|---------|-----|
| WebApp | `http://localhost:4200` |
| WebAPI | `http://localhost:5000` |
| SQL Server | `localhost:1433` |
| Redis | `localhost:6379` |
| RabbitMQ | `localhost:5672` |

## Rules

1. **ALWAYS** run `acceptance_tests.ps1` - NEVER skip
2. **NEVER** skip failing tests - fix the root cause
3. **ALWAYS** return to Development if tests fail:
   - Update label back to `Development`
   - Fix the issue
   - Re-run `build_and_test.ps1`
   - Return to Functional Validation
4. **ALWAYS** manually verify acceptance criteria if no acceptance tests were designed

## Troubleshooting

**Docker Issues:**
```powershell
docker info          # Check status
docker ps            # View running containers
docker logs <name>   # View container logs
```

**Test Failures:**
1. Check test output for failure messages
2. Review test design for expected behavior
3. Verify deployed application state
4. Check health endpoint: `GET http://localhost:5000/health`

**Deployment Issues:**
```powershell
cd environments\local
.\deploy.ps1
curl http://localhost:5000/health
```

## Gates

**BOTH MUST pass before proceeding:**

1. `scripts/build_and_test.ps1` (from Development)
2. `scripts/acceptance_tests.ps1` (this phase)

## Next Phase

**Code Review** (PR marked ready for review)
