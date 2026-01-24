namespace AcceptanceTests.Infrastructure;

/// <summary>
/// Provides HTTP client access to the containerized WebApp for API tests.
/// The WebApp runs in a Docker container alongside other infrastructure containers.
/// </summary>
public class WebAppFixture : IAsyncDisposable
{
    private readonly TestEnvironment _testEnvironment;
    private HttpClient? _httpClient;

    public WebAppFixture(TestEnvironment testEnvironment)
    {
        _testEnvironment = testEnvironment;
    }

    public async Task StartAsync()
    {
        // Start the WebApp container (infrastructure containers should already be running)
        await _testEnvironment.StartWebAppContainerAsync();
        
        // Create HTTP client pointing at the container
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_testEnvironment.WebAppUrl)
        };
    }

    public HttpClient CreateClient()
    {
        if (_httpClient == null)
            throw new InvalidOperationException("WebAppFixture must be started before creating a client");
        
        return _httpClient;
    }

    public ValueTask DisposeAsync()
    {
        _httpClient?.Dispose();
        // Cleanup is handled by TestEnvironment
        return ValueTask.CompletedTask;
    }
}
