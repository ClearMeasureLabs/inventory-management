namespace AcceptanceTests.Infrastructure;

/// <summary>
/// NUnit SetUpFixture that manages the application deployment lifecycle for acceptance tests.
/// Deploys the full stack before any tests run and tears it down after all tests complete.
/// </summary>
[SetUpFixture]
public class AcceptanceTestFixture
{
    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        await TestEnvironment.DeployAsync();
    }

    [OneTimeTearDown]
    public async Task GlobalTeardown()
    {
        await TestEnvironment.TeardownAsync();
    }
}
