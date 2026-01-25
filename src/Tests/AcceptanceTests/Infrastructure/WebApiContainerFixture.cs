using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;

namespace AcceptanceTests.Infrastructure;

/// <summary>
/// Runs the WebAPI in a Docker container for acceptance tests.
/// Expects the image to be pre-built by the build script.
/// </summary>
public class WebApiContainerFixture : IAsyncDisposable
{
    private IContainer? _container;
    private readonly TestEnvironment _testEnvironment;
    private readonly INetwork _network;

    public string ServerAddress { get; private set; } = string.Empty;
    public string ContainerAddress { get; private set; } = string.Empty;

    public WebApiContainerFixture(TestEnvironment testEnvironment, INetwork network)
    {
        _testEnvironment = testEnvironment;
        _network = network;
    }

    public async Task StartAsync()
    {
        // Use pre-built image (built by build_and_test.ps1)
        const string imageName = "webapi-acceptance-test:latest";

        // Use host.docker.internal to access services via host-mapped ports
        // This is more reliable than Docker network aliases in some environments
        const string hostGateway = "host.docker.internal";

        // Run the container
        _container = new ContainerBuilder()
            .WithImage(imageName)
            .WithNetwork(_network)
            .WithNetworkAliases("webapi")
            .WithPortBinding(8080, true)
            .WithExtraHost("host.docker.internal", "host-gateway")
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Test")
            .WithEnvironment("ASPNETCORE_URLS", "http://+:8080")
            .WithEnvironment("SqlServer__Host", hostGateway)
            .WithEnvironment("SqlServer__Port", _testEnvironment.SqlPort.ToString())
            .WithEnvironment("SqlServer__User", "sa")
            .WithEnvironment("SqlServer__Password", _testEnvironment.SqlPassword)
            .WithEnvironment("SqlServer__Database", "ivan_acceptance_db")
            .WithEnvironment("Redis__Host", hostGateway)
            .WithEnvironment("Redis__Port", _testEnvironment.RedisPort.ToString())
            .WithEnvironment("Redis__User", "default")
            .WithEnvironment("RabbitMQ__Host", hostGateway)
            .WithEnvironment("RabbitMQ__Port", _testEnvironment.RabbitMqPort.ToString())
            .WithEnvironment("RabbitMQ__User", _testEnvironment.RabbitMqUser)
            .WithEnvironment("RabbitMQ__Password", _testEnvironment.RabbitMqPassword)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilMessageIsLogged("Now listening on:"))
            .Build();

        await _container.StartAsync();

        var mappedPort = _container.GetMappedPublicPort(8080);
        ServerAddress = $"http://localhost:{mappedPort}";
        ContainerAddress = "http://webapi:8080";
    }

    public async ValueTask DisposeAsync()
    {
        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }
}
