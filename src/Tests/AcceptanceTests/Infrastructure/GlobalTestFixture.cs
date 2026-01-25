using AcceptanceTests.Infrastructure;

namespace AcceptanceTests;

/// <summary>
/// Global test fixture that initializes and shares the test environment
/// across all acceptance test classes. This ensures containers and the
/// WebApplicationFactory are only created once per test run.
/// </summary>
[SetUpFixture]
public class GlobalTestFixture
{
    private static TestEnvironment? _testEnvironment;
    private static CustomWebApplicationFactory? _webApplicationFactory;

    /// <summary>
    /// Gets the shared test environment instance.
    /// </summary>
    public static TestEnvironment TestEnvironment => _testEnvironment 
        ?? throw new InvalidOperationException("GlobalTestFixture has not been initialized");

    /// <summary>
    /// Gets the shared WebApplicationFactory instance.
    /// </summary>
    public static CustomWebApplicationFactory WebApplicationFactory => _webApplicationFactory 
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
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_webApplicationFactory != null)
            await _webApplicationFactory.DisposeAsync();

        if (_testEnvironment != null)
            await _testEnvironment.DisposeAsync();
    }
}
