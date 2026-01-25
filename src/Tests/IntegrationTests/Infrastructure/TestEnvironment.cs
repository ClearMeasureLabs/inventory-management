using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Docker.DotNet;
using Docker.DotNet.Models;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;

namespace IntegrationTests.Infrastructure;

public class TestEnvironment : IAsyncDisposable
{
    private string? _sqlContainerId;
    private RabbitMqContainer? _rabbitMqContainer;
    private RedisContainer? _redisContainer;
    private DockerClient? _dockerClient;

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
        _dockerClient = new DockerClientConfiguration().CreateClient();

        // Start SQL Server using Docker.DotNet directly to set user=0:0
        await StartSqlServerAsync();

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
            _rabbitMqContainer.StartAsync(),
            _redisContainer.StartAsync()
        );

        RedisHost = _redisContainer.Hostname;
        RedisPort = _redisContainer.GetMappedPublicPort(6379);

        RabbitMqHost = _rabbitMqContainer.Hostname;
        RabbitMqPort = _rabbitMqContainer.GetMappedPublicPort(5672);
        RabbitMqUser = "guest";
        RabbitMqPassword = "guest";
    }

    private async Task StartSqlServerAsync()
    {
        const string imageName = "mcr.microsoft.com/mssql/server:2022-latest";
        SqlPassword = "Test@Password123!";

        // Pull image if needed
        try
        {
            await _dockerClient!.Images.CreateImageAsync(
                new ImagesCreateParameters { FromImage = imageName },
                null,
                new Progress<JSONMessage>());
        }
        catch
        {
            // Image might already exist
        }

        // Create container with user=0:0 (root)
        var createResponse = await _dockerClient!.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = imageName,
            User = "0:0",
            Env = new List<string>
            {
                "ACCEPT_EULA=Y",
                $"MSSQL_SA_PASSWORD={SqlPassword}"
            },
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    { "1433/tcp", new List<PortBinding> { new PortBinding { HostPort = "0" } } }
                },
                AutoRemove = true
            },
            ExposedPorts = new Dictionary<string, EmptyStruct>
            {
                { "1433/tcp", default }
            }
        });

        _sqlContainerId = createResponse.ID;

        // Start container
        await _dockerClient.Containers.StartContainerAsync(_sqlContainerId, null);

        // Get the mapped port
        var inspectResponse = await _dockerClient.Containers.InspectContainerAsync(_sqlContainerId);
        var portBinding = inspectResponse.NetworkSettings.Ports["1433/tcp"].First();
        SqlPort = int.Parse(portBinding.HostPort);
        SqlHost = "localhost";

        // Wait for SQL Server to be ready
        await WaitForSqlServerAsync();
    }

    private async Task WaitForSqlServerAsync()
    {
        var maxAttempts = 60;
        var delay = TimeSpan.FromSeconds(2);

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                // Try connecting to SQL Server port
                using var tcpClient = new System.Net.Sockets.TcpClient();
                await tcpClient.ConnectAsync(SqlHost, SqlPort);
                
                if (tcpClient.Connected)
                {
                    // Additional wait for SQL Server to fully initialize
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    return;
                }
            }
            catch
            {
                // Ignore errors during wait
            }

            await Task.Delay(delay);
        }

        throw new TimeoutException("SQL Server did not become ready in time");
    }

    public async ValueTask DisposeAsync()
    {
        // Stop and remove all containers when the test suite completes
        if (_redisContainer != null)
            await _redisContainer.DisposeAsync();

        if (_rabbitMqContainer != null)
            await _rabbitMqContainer.DisposeAsync();

        if (_sqlContainerId != null && _dockerClient != null)
        {
            try
            {
                await _dockerClient.Containers.StopContainerAsync(_sqlContainerId, new ContainerStopParameters());
            }
            catch
            {
                // Container might already be stopped
            }
        }

        _dockerClient?.Dispose();
    }
}
