using System.Text.Json;
using AcceptanceTests.Infrastructure;

namespace AcceptanceTests;

[TestFixture]
public class HealthCheckTests
{
    private WebAppFixture _webAppFixture = null!;
    private HttpClient _httpClient = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        // Use shared test environment from global fixture
        var testEnvironment = GlobalTestFixture.TestEnvironment;

        // Create and start containerized web application
        _webAppFixture = new WebAppFixture(testEnvironment);
        await _webAppFixture.StartAsync();
        _httpClient = _webAppFixture.CreateClient();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        _httpClient?.Dispose();
        await _webAppFixture.DisposeAsync();
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
