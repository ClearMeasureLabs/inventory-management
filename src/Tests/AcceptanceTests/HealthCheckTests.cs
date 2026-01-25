using System.Text.Json;
using AcceptanceTests.Infrastructure;

namespace AcceptanceTests;

[TestFixture]
public class HealthCheckTests
{
    private WebAppFixture _webAppFixture = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Use shared WebApplicationFactory from global fixture
        _webAppFixture = new WebAppFixture(GlobalTestFixture.WebApplicationFactory);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _webAppFixture?.Dispose();
    }

    [Test]
    public async Task HealthEndpoint_AllDependenciesHealthy()
    {
        // Act
        var client = _webAppFixture.CreateClient();
        var response = await client.GetAsync("/Health");
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
