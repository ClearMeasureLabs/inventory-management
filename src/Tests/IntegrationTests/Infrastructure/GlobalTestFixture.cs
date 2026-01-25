using IntegrationTests.Infrastructure;

namespace IntegrationTests;

/// <summary>
/// Global test fixture that initializes and shares the test environment
/// across all integration test classes. This ensures containers are only
/// created once per test run, not per test class.
/// </summary>
[SetUpFixture]
public class GlobalTestFixture
{
    private static TestEnvironment? _testEnvironment;
    private static IServiceProvider? _serviceProvider;

    /// <summary>
    /// Gets the shared test environment instance.
    /// </summary>
    public static TestEnvironment TestEnvironment => _testEnvironment 
        ?? throw new InvalidOperationException("GlobalTestFixture has not been initialized");

    /// <summary>
    /// Gets the shared service provider instance.
    /// </summary>
    public static IServiceProvider ServiceProvider => _serviceProvider 
        ?? throw new InvalidOperationException("GlobalTestFixture has not been initialized");

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _testEnvironment = new TestEnvironment();
        await _testEnvironment.InitializeAsync();

        var builder = new ServiceProviderBuilder(_testEnvironment);
        _serviceProvider = await builder.BuildAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_serviceProvider is IDisposable disposable)
            disposable.Dispose();

        if (_testEnvironment != null)
            await _testEnvironment.DisposeAsync();
    }
}
