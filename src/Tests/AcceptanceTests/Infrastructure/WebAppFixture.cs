namespace AcceptanceTests.Infrastructure;

/// <summary>
/// Provides HTTP client access to the WebApp for API tests.
/// Uses WebApplicationFactory for in-process hosting with test containers for infrastructure.
/// </summary>
public class WebAppFixture : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private HttpClient? _httpClient;

    public WebAppFixture(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public HttpClient CreateClient()
    {
        _httpClient ??= _factory.CreateClient();
        return _httpClient;
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
