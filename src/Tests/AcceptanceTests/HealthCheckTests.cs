using System.Text.Json;

namespace AcceptanceTests;

[TestFixture]
public class HealthCheckTests
{
    private HttpClient _httpClient = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Use API server address from global fixture
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(GlobalTestFixture.ApiServerAddress)
        };
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _httpClient?.Dispose();
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
