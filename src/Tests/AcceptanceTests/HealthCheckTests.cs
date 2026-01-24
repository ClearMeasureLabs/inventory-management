using System.Text.Json;
using AcceptanceTests.Infrastructure;

namespace AcceptanceTests;

[TestFixture]
public class HealthCheckTests
{
    private TestEnvironment _testEnvironment = null!;
    private WebAppFixture _webAppFixture = null!;
    private HttpClient _httpClient = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        // Initialize test containers (SQL Server, Redis, RabbitMQ)
        _testEnvironment = new TestEnvironment();
        await _testEnvironment.InitializeAsync();

        // Create and start containerized web application
        _webAppFixture = new WebAppFixture(_testEnvironment);
        await _webAppFixture.StartAsync();
        _httpClient = _webAppFixture.CreateClient();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _webAppFixture.DisposeAsync();
        await _testEnvironment.DisposeAsync();
    }

    [Test]
    public async Task HealthEndpoint_AllDependenciesHealthy()
    {
        // Act
        var response = await _httpClient.GetAsync("/Health");
        var content = await response.Content.ReadAsStringAsync();
        var healthResponse = JsonSerializer.Deserialize<JsonElement>(content);

        // Assert
        var entries = healthResponse.GetProperty("entries").EnumerateArray().ToList();

        foreach (var entry in entries)
        {
            var name = entry.GetProperty("name").GetString();
            var status = entry.GetProperty("status").GetString();

            Assert.That(status, Is.EqualTo("Healthy"), $"Health check '{name}' should be Healthy");
        }
    }
}
