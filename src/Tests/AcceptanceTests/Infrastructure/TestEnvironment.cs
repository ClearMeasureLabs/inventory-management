using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;

namespace AcceptanceTests.Infrastructure;

public class TestEnvironment : IAsyncDisposable
{
    private INetwork? _network;
    private MsSqlContainer? _sqlContainer;
    private RabbitMqContainer? _rabbitMqContainer;
    private RedisContainer? _redisContainer;
    private IContainer? _webAppContainer;

    // Network alias names for container-to-container communication
    public const string SqlServerAlias = "sqlserver";
    public const string RedisAlias = "redis";
    public const string RabbitMqAlias = "rabbitmq";
    public const string WebAppAlias = "webapp";

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

    // WebApp container access
    public string WebAppUrl { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        // Create a shared network for all containers
        _network = new NetworkBuilder()
            .WithName($"acceptance-test-network-{Guid.NewGuid():N}")
            .Build();

        await _network.CreateAsync();

        // Build infrastructure containers with network aliases
        _sqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("Test@Password123!")
            .WithNetwork(_network)
            .WithNetworkAliases(SqlServerAlias)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
            .Build();

        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:management")
            .WithUsername("guest")
            .WithPassword("guest")
            .WithNetwork(_network)
            .WithNetworkAliases(RabbitMqAlias)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5672))
            .Build();

        _redisContainer = new RedisBuilder()
            .WithImage("redis:latest")
            .WithNetwork(_network)
            .WithNetworkAliases(RedisAlias)
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

    public async Task StartWebAppContainerAsync()
    {
        if (_network == null)
            throw new InvalidOperationException("TestEnvironment must be initialized before starting WebApp container");

        // Find the repository root (where the Dockerfile context is)
        var currentDir = Directory.GetCurrentDirectory();
        var repoRoot = FindRepositoryRoot(currentDir) 
            ?? throw new DirectoryNotFoundException($"Could not find repository root from {currentDir}");

        // Build and start the WebApp container
        _webAppContainer = new ContainerBuilder()
            .WithImage(new ImageFromDockerfileBuilder()
                .WithDockerfileDirectory(repoRoot)
                .WithDockerfile("src/Presentation/WebApp/Dockerfile")
                .WithDeleteIfExists(true)
                .Build())
            .WithNetwork(_network)
            .WithNetworkAliases(WebAppAlias)
            .WithPortBinding(8080, true)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithEnvironment("Environment", "Test")
            .WithEnvironment("Project__Name", "Ivan")
            .WithEnvironment("Project__Publisher", "Clear Measure")
            .WithEnvironment("Project__Version", "0.0.0")
            .WithEnvironment("SqlServer__Host", SqlServerAlias)
            .WithEnvironment("SqlServer__Port", "1433")
            .WithEnvironment("SqlServer__User", "sa")
            .WithEnvironment("SqlServer__Password", SqlPassword)
            .WithEnvironment("SqlServer__Database", "ivan_acceptance_db")
            .WithEnvironment("Redis__Host", RedisAlias)
            .WithEnvironment("Redis__Port", "6379")
            .WithEnvironment("Redis__User", "default")
            .WithEnvironment("RabbitMQ__Host", RabbitMqAlias)
            .WithEnvironment("RabbitMQ__Port", "5672")
            .WithEnvironment("RabbitMQ__User", RabbitMqUser)
            .WithEnvironment("RabbitMQ__Password", RabbitMqPassword)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilHttpRequestIsSucceeded(r => r.ForPath("/Health").ForPort(8080)))
            .Build();

        await _webAppContainer.StartAsync();

        // Get the external URL for tests to connect
        var host = _webAppContainer.Hostname;
        var port = _webAppContainer.GetMappedPublicPort(8080);
        WebAppUrl = $"http://{host}:{port}";
    }

    private static string? FindRepositoryRoot(string startPath)
    {
        var directory = new DirectoryInfo(startPath);
        while (directory != null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, ".git")))
                return directory.FullName;
            directory = directory.Parent;
        }
        return null;
    }

    public async ValueTask DisposeAsync()
    {
        if (_webAppContainer != null)
            await _webAppContainer.DisposeAsync();

        if (_sqlContainer != null)
            await _sqlContainer.DisposeAsync();

        if (_rabbitMqContainer != null)
            await _rabbitMqContainer.DisposeAsync();

        if (_redisContainer != null)
            await _redisContainer.DisposeAsync();

        if (_network != null)
            await _network.DeleteAsync();
    }
}
