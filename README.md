# Ivan - Inventory Management System

An inventory management system built with .NET 10, following clean architecture principles with CQRS.

## Overview

Ivan is a full-stack inventory management application that tracks containers and items. The system is built using:

- **Backend**: ASP.NET Core Web API with Blazor Server
- **Frontend**: Blazor WebAssembly
- **Database**: SQL Server with Entity Framework Core
- **Caching**: Redis
- **Messaging**: RabbitMQ
- **Testing**: NUnit, Playwright, Testcontainers

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
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

   This installs EF Core tools and Playwright browsers.

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

```powershell
dotnet run --project src\Presentation\WebApp
```

### Adding Database Migrations

When modifying domain entities or database mappings, create a new EF Core migration:

```powershell
.\scripts\add_migration.ps1 -MigrationName <MigrationName>
```

Migrations are generated in `src/Infrastructure/SQLServer/Migrations/`. The migration is automatically applied when the application starts.

### Common Scripts

| Script | Description |
|--------|-------------|
| `scripts\install_tools.ps1` | Install EF Core tools and Playwright browsers |
| `scripts\build_and_test.ps1` | Build solution and run all tests |
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
│   ├── WebApp/           # ASP.NET Core + Blazor Server
│   └── WebApp.Client/    # Blazor WebAssembly
│
└── Tests/
    ├── UnitTests/        # Isolated unit tests with mocks
    ├── IntegrationTests/ # Component tests with Testcontainers
    └── AcceptanceTests/  # End-to-end tests with Playwright
```

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
