using DotNet.Testcontainers.Builders;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;

namespace AcceptanceTests.Infrastructure;

public class TestEnvironment : IAsyncDisposable
{
    private MsSqlContainer? _sqlContainer;
    private RabbitMqContainer? _rabbitMqContainer;
    private RedisContainer? _redisContainer;

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

    public async Task InitializeAsync()
    {
        // Start all containers in parallel
        _sqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("Test@Password123!")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
            .Build();

        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:management")
            .WithUsername("guest")
            .WithPassword("guest")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5672))
            .Build();

        _redisContainer = new RedisBuilder()
            .WithImage("redis:latest")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(6379))
            .Build();

        await Task.WhenAll(
            _sqlContainer.StartAsync(),
            _rabbitMqContainer.StartAsync(),
            _redisContainer.StartAsync()
        );

        // Extract connection details
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
        if (_sqlContainer != null)
            await _sqlContainer.DisposeAsync();

        if (_rabbitMqContainer != null)
            await _rabbitMqContainer.DisposeAsync();

        if (_redisContainer != null)
            await _redisContainer.DisposeAsync();
    }
}
