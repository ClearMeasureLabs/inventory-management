using AcceptanceTests.Infrastructure;

namespace AcceptanceTests;

/// <summary>
/// Global test fixture that initializes and shares the test environment
/// across all acceptance test classes. This ensures containers are only
/// created once per test run, not per test class.
/// </summary>
[SetUpFixture]
public class GlobalTestFixture
{
    private static TestEnvironment? _testEnvironment;

    /// <summary>
    /// Gets the shared test environment instance.
    /// </summary>
    public static TestEnvironment TestEnvironment => _testEnvironment 
        ?? throw new InvalidOperationException("GlobalTestFixture has not been initialized");

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _testEnvironment = new TestEnvironment();
        await _testEnvironment.InitializeAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_testEnvironment != null)
            await _testEnvironment.DisposeAsync();
    }
}
