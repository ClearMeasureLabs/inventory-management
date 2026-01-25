using AcceptanceTests.Infrastructure;

namespace AcceptanceTests;

/// <summary>
/// Global test fixture that initializes and shares the test environment
/// across all acceptance test classes. This ensures containers and the
/// API/Angular servers are only created once per test run.
/// 
/// All components run in Docker containers:
/// - Infrastructure: SQL Server, Redis, RabbitMQ
/// - WebAPI: .NET container
/// - Angular webapp: Node/nginx container
/// </summary>
[SetUpFixture]
public class GlobalTestFixture
{
    private static TestEnvironment? _testEnvironment;
    private static WebApiContainerFixture? _webApiContainerFixture;
    private static AngularContainerFixture? _angularContainerFixture;

    /// <summary>
    /// Gets the shared test environment instance.
    /// </summary>
    public static TestEnvironment TestEnvironment => _testEnvironment 
        ?? throw new InvalidOperationException("GlobalTestFixture has not been initialized");

    /// <summary>
    /// Gets the API server address for HTTP client calls.
    /// </summary>
    public static string ApiServerAddress => _webApiContainerFixture?.ServerAddress 
        ?? throw new InvalidOperationException("GlobalTestFixture has not been initialized");

    /// <summary>
    /// Gets the Angular app server address for Playwright tests.
    /// </summary>
    public static string AngularAppAddress => _angularContainerFixture?.ServerAddress 
        ?? throw new InvalidOperationException("GlobalTestFixture has not been initialized");

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        // Start infrastructure containers first (SQL Server, Redis, RabbitMQ)
        _testEnvironment = new TestEnvironment();
        await _testEnvironment.InitializeAsync();

        // Wait for infrastructure containers to be fully ready
        // This ensures network aliases are properly registered and services are accepting connections
        await Task.Delay(TimeSpan.FromSeconds(15));

        // Start WebAPI container
        _webApiContainerFixture = new WebApiContainerFixture(_testEnvironment, _testEnvironment.Network);
        await _webApiContainerFixture.StartAsync();

        // Start Angular app container - use WebAPI's external address for proxying
        // Angular container will use host.docker.internal to reach the WebAPI via host-mapped port
        _angularContainerFixture = new AngularContainerFixture(_webApiContainerFixture.ServerAddress, _testEnvironment.Network);
        await _angularContainerFixture.StartAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_angularContainerFixture != null)
            await _angularContainerFixture.DisposeAsync();

        if (_webApiContainerFixture != null)
            await _webApiContainerFixture.DisposeAsync();

        if (_testEnvironment != null)
            await _testEnvironment.DisposeAsync();
    }
}
