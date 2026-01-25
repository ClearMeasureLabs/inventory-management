using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;

namespace AcceptanceTests.Infrastructure;

/// <summary>
/// Manages infrastructure test containers (SQL Server, Redis, RabbitMQ) and the shared network.
/// All containers are connected to a shared network for inter-container communication.
/// </summary>
public class TestEnvironment : IAsyncDisposable
{
    private MsSqlContainer? _sqlContainer;
    private RabbitMqContainer? _rabbitMqContainer;
    private RedisContainer? _redisContainer;
    private INetwork? _network;

    // Connection details for external access (from test host)
    public string SqlConnectionString { get; private set; } = string.Empty;
    public string SqlHost { get; private set; } = string.Empty;
    public int SqlPort { get; private set; }
    public string SqlPassword { get; private set; } = string.Empty;

    public string RedisHost { get; private set; } = string.Empty;
    public int RedisPort { get; private set; }

    public string RabbitMqHost { get; private set; } = string.Empty;
    public int RabbitMqPort { get; private set; }
    public string RabbitMqUser { get; private set; } = string.Empty;
    public string RabbitMqPassword { get; private set; } = string.Empty;

    // Container aliases for inter-container communication
    public string SqlContainerAlias => "sqlserver";
    public string RedisContainerAlias => "redis";
    public string RabbitMqContainerAlias => "rabbitmq";

    // Shared network for all containers
    public INetwork Network => _network ?? throw new InvalidOperationException("Network not initialized");

    public async Task InitializeAsync()
    {
        // Create a shared network for all containers
        _network = new NetworkBuilder()
            .WithName($"test-network-{Guid.NewGuid():N}")
            .WithCleanUp(true)
            .Build();

        await _network.CreateAsync();

        // Build infrastructure containers with network aliases
        _sqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("Test@Password123!")
            .WithNetwork(_network)
            .WithNetworkAliases(SqlContainerAlias)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
            .Build();

        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:management")
            .WithUsername("guest")
            .WithPassword("guest")
            .WithNetwork(_network)
            .WithNetworkAliases(RabbitMqContainerAlias)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5672))
            .Build();

        _redisContainer = new RedisBuilder()
            .WithImage("redis:latest")
            .WithNetwork(_network)
            .WithNetworkAliases(RedisContainerAlias)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(6379))
            .Build();

        // Start infrastructure containers in parallel
        await Task.WhenAll(
            _sqlContainer.StartAsync(),
            _rabbitMqContainer.StartAsync(),
            _redisContainer.StartAsync()
        );

        // Extract connection details for external access
        SqlConnectionString = _sqlContainer.GetConnectionString();
        SqlHost = _sqlContainer.Hostname;
        SqlPort = _sqlContainer.GetMappedPublicPort(1433);
        SqlPassword = "Test@Password123!";

        RedisHost = _redisContainer.Hostname;
        RedisPort = _redisContainer.GetMappedPublicPort(6379);

        RabbitMqHost = _rabbitMqContainer.Hostname;
        RabbitMqPort = _rabbitMqContainer.GetMappedPublicPort(5672);
        RabbitMqUser = "guest";
        RabbitMqPassword = "guest";
    }

    public async ValueTask DisposeAsync()
    {
        // Stop and remove all containers when the test suite completes
        if (_redisContainer != null)
            await _redisContainer.DisposeAsync();

        if (_rabbitMqContainer != null)
            await _rabbitMqContainer.DisposeAsync();

        if (_sqlContainer != null)
            await _sqlContainer.DisposeAsync();

        if (_network != null)
            await _network.DisposeAsync();
    }
}
