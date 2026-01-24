using System.Text.Json;
using AcceptanceTests.Infrastructure;
using Microsoft.Playwright;

namespace AcceptanceTests;

[TestFixture]
public class HealthCheckTests
{
    private TestEnvironment _testEnvironment = null!;
    private WebAppFixture _webAppFixture = null!;
    private HttpClient _httpClient = null!;
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        // Initialize test containers
        _testEnvironment = new TestEnvironment();
        await _testEnvironment.InitializeAsync();

        // Create web application with test containers
        _webAppFixture = new WebAppFixture(_testEnvironment);
        _httpClient = _webAppFixture.CreateClient();

        // Initialize Playwright for UI tests
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        _httpClient?.Dispose();
        _webAppFixture?.Dispose();
        await _testEnvironment.DisposeAsync();

        await _browser.DisposeAsync();
        _playwright.Dispose();
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
