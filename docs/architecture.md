# Architecture

Ivan is an inventory management system built with .NET 10 and Angular 19.

## The Big Picture

The codebase follows **clean architecture** - business logic lives in the center, infrastructure details live on the outside. This keeps the core code testable and independent of frameworks, databases, and UI.

```
┌─────────────────────────────────────────────────────────┐
│  Presentation    WebAPI (ASP.NET) + WebApp (Angular)    │
├─────────────────────────────────────────────────────────┤
│  Infrastructure  SQL Server, Redis, RabbitMQ            │
├─────────────────────────────────────────────────────────┤
│  Application     Business logic, commands, queries      │
├─────────────────────────────────────────────────────────┤
│  Domain          Entities (Container, Item)             │
└─────────────────────────────────────────────────────────┘
```

**The rule:** Dependencies only point inward. The Domain layer knows nothing about databases or HTTP. The Application layer knows nothing about SQL Server or Angular. This is enforced by project references.

## Where Things Live

```
src/
├── Core/
│   ├── Domain/           # Entities only, zero dependencies
│   └── Application/      # Business logic, CQRS handlers, DTOs
├── Infrastructure/
│   ├── Bootstrap/        # Dependency injection setup
│   ├── SQLServer/        # Database (EF Core)
│   ├── Redis/            # Caching
│   └── RabbitMQ/         # Event publishing
├── Presentation/
│   ├── WebAPI/           # REST API (ASP.NET Core)
│   └── WebApp/           # UI (Angular 19)
└── Tests/
    ├── UnitTests/
    ├── IntegrationTests/
    └── AcceptanceTests/
```

## Adding a New Feature

Most features follow the CQRS pattern. Here's how to add a new command (like "archive a container"):

### 1. Create the command structure

```
Application/Features/Containers/ArchiveContainer/
├── ArchiveContainerCommand.cs        # The request (input)
├── ArchiveContainerCommandHandler.cs # The logic
├── IArchiveContainerCommandHandler.cs # Interface for DI
└── ContainerArchivedEvent.cs         # Event to publish (optional)
```

### 2. Write the command (the input)

```csharp
// ArchiveContainerCommand.cs
public record ArchiveContainerCommand(Guid ContainerId);
```

### 3. Write the handler (the logic)

```csharp
// ArchiveContainerCommandHandler.cs
public class ArchiveContainerCommandHandler : IArchiveContainerCommandHandler
{
    private readonly IRepository _repository;
    private readonly ICache _cache;
    private readonly IEventHub _eventHub;

    public async Task<ContainerDto> HandleAsync(ArchiveContainerCommand command)
    {
        // 1. Validate
        var container = await _repository.GetContainerAsync(command.ContainerId)
            ?? throw new ValidationException("Container not found");

        // 2. Modify entity
        container.IsArchived = true;

        // 3. Persist
        await _repository.UpdateContainerAsync(container);

        // 4. Invalidate cache
        await _cache.RemoveAsync($"Container:{command.ContainerId}");

        // 5. Publish event
        await _eventHub.PublishAsync(new ContainerArchivedEvent(container.ContainerId));

        // 6. Return DTO (never the entity)
        return new ContainerDto(container);
    }
}
```

### 4. Register with DI

Add to the facade interface and implementation in `Containers.cs` / `IContainers.cs`.

### 5. Add the API endpoint

```csharp
// ContainersController.cs
[HttpPost("{containerId}/archive")]
public async Task<ActionResult<ContainerDto>> Archive(Guid containerId)
{
    var result = await _containers.ArchiveAsync(new ArchiveContainerCommand(containerId));
    return Ok(result);
}
```

## The Layers Explained

### Domain Layer

Pure C# classes representing business concepts. No attributes, no framework dependencies, no database concerns.

```csharp
public class Container
{
    public Guid ContainerId { get; set; }
    public string Name { get; set; }          // max 200 chars
    public string Description { get; set; }   // max 1000 chars
    public List<ContainerItem> Items { get; set; }
}
```

**Location:** `src/Core/Domain/Entities/`

### Application Layer

Business logic organized by feature. Each feature has commands (writes) and queries (reads).

**Key interfaces defined here:**
- `IRepository` - data access
- `ICache` - caching
- `IEventHub` - event publishing

These interfaces are implemented by Infrastructure, but defined here so the business logic doesn't depend on specific technologies.

**Location:** `src/Core/Application/`

### Infrastructure Layer

Implementations of the interfaces using real technologies:

| Interface | Implementation | Technology |
|-----------|----------------|------------|
| `IRepository` | `SQLServerRepository` | EF Core + SQL Server |
| `ICache` | `RedisCache` | StackExchange.Redis |
| `IEventHub` | `RabbitMQEventHub` | RabbitMQ.Client |

The **Bootstrap** project wires everything together with dependency injection. Look at `DependencyInjection.cs` to see how services are registered.

**Location:** `src/Infrastructure/`

### Presentation Layer

**WebAPI** - REST endpoints that receive HTTP requests, call Application layer handlers, and return responses.

**WebApp** - Angular SPA that calls the API. Uses standalone components (no NgModules).

```
WebApp/src/app/
├── components/          # UI components
│   ├── home/           # Main view
│   └── *-modal/        # Dialog components
├── services/           # HTTP clients
└── models/             # TypeScript interfaces
```

**Location:** `src/Presentation/`

## Configuration

Settings come from JSON files that get merged:

```
environments/global.config.json     # Shared (project name, DB name)
environments/local/local.config.json # Local dev (connection strings)
```

Environment variables override JSON values. The Bootstrap layer handles the merging.

## Database

EF Core with SQL Server. Migrations live in `Infrastructure/SQLServer/Migrations/`.

Add a migration:
```powershell
.\scripts\add_migration.ps1 -MigrationName AddArchivedFlag
```

Migrations run automatically on app startup.

## Testing

| What to test | Where | How |
|--------------|-------|-----|
| Handler logic in isolation | `UnitTests/` | Mock IRepository, ICache, etc. |
| Handler + real database | `IntegrationTests/` | Testcontainers spins up SQL Server |
| Angular components | `*.spec.ts` next to component | Jasmine + Karma |
| Full user workflows | `AcceptanceTests/` | Playwright against deployed app |

Run all tests:
```powershell
.\scripts\build_and_test.ps1
```

## Common Patterns

### Validation

Throw `ValidationException` for bad input. The API catches these and returns 400 Bad Request.

```csharp
if (string.IsNullOrWhiteSpace(command.Name))
    throw new ValidationException("Name is required");
```

### DTOs

Never return domain entities from handlers. Map to DTOs:

```csharp
// Good
return new ContainerDto(container);

// Bad - exposes internal structure
return container;
```

### Caching

Cache keys follow the pattern `{EntityType}:{Id}`:

```csharp
await _cache.SetAsync($"Container:{container.ContainerId}", containerDto);
await _cache.RemoveAsync($"Container:{container.ContainerId}");
```

### Events

Publish events for significant state changes. Other services can subscribe via RabbitMQ:

```csharp
await _eventHub.PublishAsync(new ContainerCreatedEvent(container.ContainerId));
```

## Local Development

Start the infrastructure:
```powershell
cd environments\local
.\provision.ps1
```

This starts:
| Service | Port |
|---------|------|
| SQL Server | 1433 |
| Redis | 6379 |
| RabbitMQ | 5672 (management: 15672) |

Run the API:
```powershell
dotnet run --project src/Presentation/WebAPI
```

Run the Angular app:
```powershell
cd src/Presentation/WebApp
npm start
```

The app runs at http://localhost:4200, API at http://localhost:5000.
