using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;

namespace AcceptanceTests.Infrastructure;

/// <summary>
/// Builds and runs the Angular webapp in a Docker container for acceptance tests.
/// </summary>
public class AngularContainerFixture : IAsyncDisposable
{
    private IFutureDockerImage? _image;
    private IContainer? _container;
    private readonly string _apiUrl;
    private readonly INetwork _network;

    public string ServerAddress { get; private set; } = string.Empty;

    public AngularContainerFixture(string apiUrl, INetwork network)
    {
        _apiUrl = apiUrl;
        _network = network;
    }

    public async Task StartAsync()
    {
        var webappPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "..", "Presentation", "webapp"));
        
        // Update the environment.ts with the correct API URL before building
        await UpdateEnvironmentFileAsync(webappPath);

        // Build the Angular Docker image
        _image = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(webappPath)
            .WithDockerfile("Dockerfile")
            .WithName($"angular-test:{Guid.NewGuid():N}")
            .WithCleanUp(true)
            .Build();

        await _image.CreateAsync();

        // Run the container
        _container = new ContainerBuilder()
            .WithImage(_image)
            .WithNetwork(_network)
            .WithNetworkAliases("angular")
            .WithPortBinding(80, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPath("/").ForPort(80)))
            .Build();

        await _container.StartAsync();

        var mappedPort = _container.GetMappedPublicPort(80);
        ServerAddress = $"http://localhost:{mappedPort}";
    }

    private async Task UpdateEnvironmentFileAsync(string webappPath)
    {
        var envFilePath = Path.Combine(webappPath, "src", "environments", "environment.ts");
        
        var content = $@"export const environment = {{
  production: false,
  apiUrl: '{_apiUrl}'
}};
";
        
        await File.WriteAllTextAsync(envFilePath, content);
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
