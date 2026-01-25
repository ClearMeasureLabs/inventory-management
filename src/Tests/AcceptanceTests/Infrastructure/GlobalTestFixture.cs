using AcceptanceTests.Infrastructure;

namespace AcceptanceTests;

/// <summary>
/// Global test fixture that initializes and shares the test environment
/// across all acceptance test classes. This ensures containers and the
/// API/Angular servers are only created once per test run.
/// </summary>
[SetUpFixture]
public class GlobalTestFixture
{
    private static TestEnvironment? _testEnvironment;
    private static CustomWebApplicationFactory? _webApplicationFactory;
    private static ApiServerFixture? _apiServerFixture;
    private static AngularAppFixture? _angularAppFixture;

    /// <summary>
    /// Gets the shared test environment instance.
    /// </summary>
    public static TestEnvironment TestEnvironment => _testEnvironment 
        ?? throw new InvalidOperationException("GlobalTestFixture has not been initialized");

    /// <summary>
    /// Gets the shared WebApplicationFactory instance for API testing.
    /// </summary>
    public static CustomWebApplicationFactory WebApplicationFactory => _webApplicationFactory 
        ?? throw new InvalidOperationException("GlobalTestFixture has not been initialized");

    /// <summary>
    /// Gets the API server address for HTTP client calls.
    /// </summary>
    public static string ApiServerAddress => _apiServerFixture?.ServerAddress 
        ?? throw new InvalidOperationException("GlobalTestFixture has not been initialized");

    /// <summary>
    /// Gets the Angular app server address for Playwright tests.
    /// </summary>
    public static string AngularAppAddress => _angularAppFixture?.ServerAddress 
        ?? throw new InvalidOperationException("GlobalTestFixture has not been initialized");

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        // Start infrastructure containers first
        _testEnvironment = new TestEnvironment();
        await _testEnvironment.InitializeAsync();

        // Create WebApplicationFactory configured to use the test containers
        _webApplicationFactory = new CustomWebApplicationFactory(_testEnvironment);
        
        // Ensure the host is started
        _ = _webApplicationFactory.Server;

        // Start API server for Playwright tests
        _apiServerFixture = new ApiServerFixture(_testEnvironment);
        await _apiServerFixture.StartAsync();

        // Start Angular app server
        _angularAppFixture = new AngularAppFixture();
        await _angularAppFixture.StartAsync(_apiServerFixture.ServerAddress);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_angularAppFixture != null)
            await _angularAppFixture.DisposeAsync();

        if (_apiServerFixture != null)
            await _apiServerFixture.DisposeAsync();

        if (_webApplicationFactory != null)
            await _webApplicationFactory.DisposeAsync();

        if (_testEnvironment != null)
            await _testEnvironment.DisposeAsync();
    }
}
