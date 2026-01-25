using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;

namespace AcceptanceTests.Infrastructure;

/// <summary>
/// Runs the Angular webapp in a Docker container for acceptance tests.
/// Expects the image to be pre-built by the build script.
/// </summary>
public class AngularContainerFixture : IAsyncDisposable
{
    private IContainer? _container;
    private readonly INetwork _network;

    public string ServerAddress { get; private set; } = string.Empty;

    public AngularContainerFixture(INetwork network)
    {
        _network = network;
    }

    public async Task StartAsync()
    {
        // Use pre-built image (built by build_and_test.ps1)
        const string imageName = "angular-acceptance-test:latest";

        // Run the container
        _container = new ContainerBuilder()
            .WithImage(imageName)
            .WithNetwork(_network)
            .WithNetworkAliases("angular")
            .WithPortBinding(80, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPath("/").ForPort(80)))
            .Build();

        await _container.StartAsync();

        var mappedPort = _container.GetMappedPublicPort(80);
        ServerAddress = $"http://localhost:{mappedPort}";
    }

    public async ValueTask DisposeAsync()
    {
        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }
}
