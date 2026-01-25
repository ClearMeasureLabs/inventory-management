using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;

namespace AcceptanceTests.Infrastructure;

/// <summary>
/// Builds and runs the WebAPI in a Docker container for acceptance tests.
/// </summary>
public class WebApiContainerFixture : IAsyncDisposable
{
    private IFutureDockerImage? _image;
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
        var srcPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", ".."));
        
        // Build the WebAPI Docker image
        _image = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(srcPath)
            .WithDockerfile("Presentation/WebAPI/Dockerfile")
            .WithName($"webapi-test:{Guid.NewGuid():N}")
            .WithCleanUp(true)
            .Build();

        await _image.CreateAsync();

        // Run the container
        _container = new ContainerBuilder()
            .WithImage(_image)
            .WithNetwork(_network)
            .WithNetworkAliases("webapi")
            .WithPortBinding(8080, true)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Test")
            .WithEnvironment("ASPNETCORE_URLS", "http://+:8080")
            .WithEnvironment("SqlServer__Host", _testEnvironment.SqlContainerAlias)
            .WithEnvironment("SqlServer__Port", "1433")
            .WithEnvironment("SqlServer__User", "sa")
            .WithEnvironment("SqlServer__Password", _testEnvironment.SqlPassword)
            .WithEnvironment("SqlServer__Database", "ivan_acceptance_db")
            .WithEnvironment("Redis__Host", _testEnvironment.RedisContainerAlias)
            .WithEnvironment("Redis__Port", "6379")
            .WithEnvironment("Redis__User", "default")
            .WithEnvironment("RabbitMQ__Host", _testEnvironment.RabbitMqContainerAlias)
            .WithEnvironment("RabbitMQ__Port", "5672")
            .WithEnvironment("RabbitMQ__User", _testEnvironment.RabbitMqUser)
            .WithEnvironment("RabbitMQ__Password", _testEnvironment.RabbitMqPassword)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPath("/Health").ForPort(8080)))
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

        if (_image != null)
        {
            await _image.DisposeAsync();
        }
    }
}
