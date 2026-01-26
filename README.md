# Ivan - Inventory Management System

An inventory management system built with .NET 10 and Angular, following clean architecture principles with CQRS.

## Overview

Ivan is a full-stack inventory management application that tracks containers and items. The system is built using:

- **Backend**: ASP.NET Core Web API
- **Frontend**: Angular 19 with Bootstrap 5
- **Database**: SQL Server with Entity Framework Core
- **Caching**: Redis
- **Messaging**: RabbitMQ
- **Testing**: NUnit, Jasmine/Karma, Testcontainers

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/) (with npm)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Google Chrome](https://www.google.com/chrome/) (for Angular tests)
- PowerShell

### Initial Setup

1. **Clone the repository**

   ```powershell
   git clone <repository-url>
   cd inventory-management
   ```

2. **Install development tools**

   ```powershell
   .\scripts\install_tools.ps1
   ```

   This installs EF Core tools and Angular dependencies.

3. **Start infrastructure services**

   ```powershell
   cd environments\local
   .\provision.ps1
   ```

   This starts SQL Server, Redis, RabbitMQ, and Redis Insight via Docker Compose.

4. **Build and run tests**

   ```powershell
   .\scripts\build_and_test.ps1
   ```

### Running the Application

**Backend API:**

```powershell
dotnet run --project src\Presentation\WebAPI
```

The API will be available at `https://localhost:5000`.

**Frontend (Angular):**

```powershell
cd src\Presentation\webapp
npm start
```

The Angular app will be available at `http://localhost:4200`.

### Adding Database Migrations

When modifying domain entities or database mappings, create a new EF Core migration:

```powershell
.\scripts\add_migration.ps1 -MigrationName <MigrationName>
```

Migrations are generated in `src/Infrastructure/SQLServer/Migrations/`. The migration is automatically applied when the application starts.

### Common Scripts

| Script | Description |
|--------|-------------|
| `scripts\install_tools.ps1` | Install EF Core tools and Angular dependencies |
| `scripts\build_and_test.ps1` | Build Angular app, build solution, and run all tests |
| `scripts\add_migration.ps1 -MigrationName <name>` | Add a new EF Core migration |
| `environments\local\provision.ps1` | Start local infrastructure via Docker |

### Local Services

After running `provision.ps1`, these services are available:

| Service | URL/Port | Credentials |
|---------|----------|-------------|
| SQL Server | `localhost:1433` | sa / (see local.config.json) |
| RabbitMQ | `localhost:5672` | admin / (see local.config.json) |
| RabbitMQ Management | `localhost:15672` | admin / (see local.config.json) |
| Redis | `localhost:6379` | - |
| Redis Insight | `localhost:5540` | - |

## Architecture

The project follows **clean/onion architecture** with **CQRS** (Command Query Responsibility Segregation).

### Project Structure

```
src/
├── Core/
│   ├── Domain/           # Domain entities (no dependencies)
│   └── Application/      # Features, DTOs, interfaces (depends on Domain)
│
├── Infrastructure/
│   ├── Bootstrap/        # Dependency injection configuration
│   ├── SQLServer/        # EF Core repository implementation
│   ├── Redis/            # Cache implementation
│   └── RabbitMQ/         # Event hub implementation
│
├── Presentation/
│   ├── WebAPI/           # ASP.NET Core Web API
│   └── webapp/           # Angular application
│
└── Tests/
    ├── UnitTests/        # Isolated unit tests with mocks (NUnit)
    └── IntegrationTests/ # Component tests with Testcontainers (NUnit)
```

The Angular application includes its own component tests using Jasmine/Karma.

### Dependency Flow

Dependencies point inward—outer layers depend on inner layers:

```
Presentation → Infrastructure → Application → Domain
```

### CQRS Pattern

Features are organized by domain concept with commands and handlers:

```
Application/Features/{Entity}/
├── {Entity}s.cs                         # Facade
├── I{Entity}s.cs                        # Facade interface
└── {Action}{Entity}/
    ├── {Action}{Entity}Command.cs       # Command request
    ├── {Action}{Entity}CommandHandler.cs
    └── I{Action}{Entity}CommandHandler.cs
```

**Request flow:**

```
API Request → Command/Query → Handler → DTO → API Response
```

### API and Frontend Communication

The Angular frontend communicates with the .NET WebAPI via HTTP:

- **Development**: Angular uses the API URL configured in `src/environments/environment.ts`
- **Production**: Configure the API URL in `src/environments/environment.prod.ts`

### Configuration

Configuration is layered by environment:

```
environments/
├── global.config.json      # Shared settings (project name, database name)
└── local/
    ├── local.config.json   # Local environment settings
    ├── docker-compose.yml  # Infrastructure services
    └── init-db.sql         # Database initialization
```

### Angular Application

The Angular app is located at `src/Presentation/webapp/` and uses:

- **Angular 19** with standalone components
- **Bootstrap 5** for styling
- **RxJS** for reactive programming
- **Jasmine/Karma** for component testing

Key directories:
- `src/app/components/` - UI components (nav, home, modals)
- `src/app/services/` - HTTP services for API communication
- `src/app/models/` - TypeScript interfaces for API contracts
- `src/environments/` - Environment-specific configuration

## Testing

The project uses a layered testing strategy:

### Unit Tests (.NET)

Located in `src/Tests/UnitTests/`. Test isolated behavior using mocks.

```powershell
dotnet test src/Tests/UnitTests
```

### Integration Tests (.NET)

Located in `src/Tests/IntegrationTests/`. Test components with real infrastructure using Testcontainers.

```powershell
dotnet test src/Tests/IntegrationTests
```

### Angular Component Tests

Located alongside components in `src/Presentation/webapp/src/app/`. Test UI components with Jasmine/Karma.

```powershell
cd src/Presentation/webapp
npm test -- --watch=false --browsers=ChromeHeadless
```

### Run All Tests

```powershell
.\scripts\build_and_test.ps1
```

This runs all test suites: .NET unit tests, .NET integration tests, and Angular component tests.

## Automated PR Review

This repository uses a Cursor Cloud Agent to automatically review pull requests when they are marked ready for review.

### How It Works

1. When a PR targeting `main` is marked "Ready for review", a GitHub Action triggers
2. The action spawns a Cursor Cloud Agent via the API
3. The agent reviews the PR against the checklist in the PR template
4. The agent references the linked issue to understand requirements
5. The agent posts a review comment with findings and either approves or requests changes

### PR Template

All PRs should use the template at `.github/PULL_REQUEST_TEMPLATE.md`, which includes:
- Summary and related issue sections
- Comprehensive review checklist covering architecture, testing, documentation, and code quality
- Reviewer notes section for additional context

### Requirements

- PRs must target the `main` branch
- PRs must link to an issue using "Closes #[issue-number]"
- The `CURSOR_API_KEY` secret must be configured in the repository

## Development with Cursor AI

This repository includes configuration for [Cursor](https://cursor.com/) AI-assisted development in the `.cursor/rules/` directory. These rules ensure consistent code quality and workflow adherence.

| Rule File | Purpose |
|-----------|---------|
| `architecture-rules.mdc` | Layer boundaries and CQRS patterns |
| `automated-testing-rules.mdc` | Required test coverage and testing guidelines |
| `code-review-rules.mdc` | Automated PR review process and checklist |
| `github-issue-rules.mdc` | Issue creation interview process |
| `github-issue-implementation-rules.mdc` | Phase workflow for implementing issues |
| `cursor-cloud-agent.mdc` | Operational constraints for Cloud Agent |

When using Cursor to contribute, the AI will automatically follow these rules to maintain project standards.
