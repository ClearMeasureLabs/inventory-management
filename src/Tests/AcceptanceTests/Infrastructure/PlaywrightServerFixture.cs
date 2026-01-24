namespace AcceptanceTests.Infrastructure;

/// <summary>
/// Provides access to the containerized WebApp server for Playwright browser tests.
/// The WebApp runs in a Docker container alongside other infrastructure containers.
/// </summary>
public class PlaywrightServerFixture : IAsyncDisposable
{
    private readonly TestEnvironment _testEnvironment;

    public string ServerAddress => _testEnvironment.WebAppUrl;

    public PlaywrightServerFixture(TestEnvironment testEnvironment)
    {
        _testEnvironment = testEnvironment;
    }

    public async Task StartAsync()
    {
        // Start the WebApp container (infrastructure containers should already be running)
        await _testEnvironment.StartWebAppContainerAsync();
    }

    public ValueTask DisposeAsync()
    {
        // Cleanup is handled by TestEnvironment
        return ValueTask.CompletedTask;
    }
}
