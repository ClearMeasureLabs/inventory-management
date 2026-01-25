using AcceptanceTests.Infrastructure;
using Application.Features.Containers.CreateContainer;
using Microsoft.Playwright;
using Shouldly;
using System.Net.Http.Json;

namespace AcceptanceTests;

[TestFixture]
public class AddContainerTests
{
    private PlaywrightServerFixture _serverFixture = null!;
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private string _baseUrl = null!;
    private HttpClient _httpClient = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        // Use shared test environment from global fixture
        _serverFixture = new PlaywrightServerFixture(GlobalTestFixture.TestEnvironment);
        await _serverFixture.StartAsync();
        _baseUrl = _serverFixture.ServerAddress;

        // Create HTTP client for API calls
        _httpClient = new HttpClient { BaseAddress = new Uri(_baseUrl) };

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
        await _browser.DisposeAsync();
        _playwright.Dispose();
        _httpClient.Dispose();

        await _serverFixture.DisposeAsync();
    }

    [Test]
    public async Task CreateContainerApi_WithValidName_ShouldReturn201Created()
    {
        // Arrange
        var command = new CreateContainerCommand
        {
            Name = $"API-Container-{Guid.NewGuid():N}"
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/containers", command);

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Created);
    }

    [Test]
    public async Task CreateContainerApi_WithEmptyName_ShouldReturn400BadRequest()
    {
        // Arrange
        var command = new CreateContainerCommand
        {
            Name = string.Empty
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/containers", command);

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task CreateContainerApi_AfterSuccess_ShouldShowContainerInUI()
    {
        // Arrange - Create container via API
        var containerName = $"UI-Container-{Guid.NewGuid():N}";
        var command = new CreateContainerCommand
        {
            Name = containerName
        };

        var response = await _httpClient.PostAsJsonAsync("/api/containers", command);
        response.EnsureSuccessStatusCode();

        // Act - Navigate to homepage and verify container appears
        var page = await _browser.NewPageAsync();
        await page.GotoAsync(_baseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Wait for spinner to disappear
        var spinner = page.Locator(".spinner-border");
        await spinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 30000 });

        // Assert - Container should appear in the table
        var containerRow = page.Locator($"td:has-text('{containerName}')");
        await Expect(containerRow).ToBeVisibleAsync(new() { Timeout = 10000 });

        await page.CloseAsync();
    }

    [Test]
    public async Task HomePage_ShouldShowAddContainerButton()
    {
        // Arrange
        var page = await _browser.NewPageAsync();
        await page.GotoAsync(_baseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Wait for spinner to disappear
        var spinner = page.Locator(".spinner-border");
        await spinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 30000 });

        // Assert
        var addButton = page.Locator("button:has-text('Add Container')");
        await Expect(addButton).ToBeVisibleAsync(new() { Timeout = 10000 });

        await page.CloseAsync();
    }

    [Test]
    public async Task CreateContainerApi_WithValidName_ShouldReturnContainerData()
    {
        // Arrange
        var containerName = $"Data-Container-{Guid.NewGuid():N}";
        var command = new CreateContainerCommand
        {
            Name = containerName
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/containers", command);
        var result = await response.Content.ReadFromJsonAsync<ContainerApiResponse>();

        // Assert
        result.ShouldNotBeNull();
        result.ContainerId.ShouldBeGreaterThan(0);
        result.Name.ShouldBe(containerName);
    }

    private class ContainerApiResponse
    {
        public int ContainerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    private static ILocatorAssertions Expect(ILocator locator) => Assertions.Expect(locator);
}
