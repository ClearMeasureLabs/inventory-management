using AcceptanceTests.Infrastructure;
using Microsoft.Playwright;
using Shouldly;
using System.Net.Http.Json;
using WebAPI.Contracts;

namespace AcceptanceTests;

[TestFixture]
public class AddContainerTests
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private string _apiBaseUrl = null!;
    private string _angularBaseUrl = null!;
    private HttpClient _httpClient = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        // Use the API and Angular URLs from global fixture
        _apiBaseUrl = GlobalTestFixture.ApiServerAddress;
        _angularBaseUrl = GlobalTestFixture.AngularAppAddress;

        // Create HTTP client for API calls
        _httpClient = new HttpClient { BaseAddress = new Uri(_apiBaseUrl) };

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
    }

    [Test]
    public async Task CreateContainerApi_WithValidName_ShouldReturn201Created()
    {
        // Arrange
        var request = new CreateContainerRequest
        {
            Name = $"API-Container-{Guid.NewGuid():N}"
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/containers", request);

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Created);
    }

    [Test]
    public async Task CreateContainerApi_WithEmptyName_ShouldReturn400BadRequest()
    {
        // Arrange
        var request = new CreateContainerRequest
        {
            Name = string.Empty
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/containers", request);

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task CreateContainerApi_AfterSuccess_ShouldShowContainerInUI()
    {
        // Arrange - Create container via API
        var containerName = $"UI-Container-{Guid.NewGuid():N}";
        var request = new CreateContainerRequest
        {
            Name = containerName
        };

        var response = await _httpClient.PostAsJsonAsync("/api/containers", request);
        response.EnsureSuccessStatusCode();

        // Act - Navigate to homepage and verify container appears
        var page = await _browser.NewPageAsync();
        await page.GotoAsync(_angularBaseUrl);
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
        await page.GotoAsync(_angularBaseUrl);
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
        var request = new CreateContainerRequest
        {
            Name = containerName
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/containers", request);
        var result = await response.Content.ReadFromJsonAsync<ContainerResponse>();

        // Assert
        result.ShouldNotBeNull();
        result.ContainerId.ShouldBeGreaterThan(0);
        result.Name.ShouldBe(containerName);
    }

    [Test]
    public async Task GetAllContainersApi_ShouldReturnContainersList()
    {
        // Arrange - Create a container first
        var containerName = $"List-Container-{Guid.NewGuid():N}";
        var createRequest = new CreateContainerRequest
        {
            Name = containerName
        };
        await _httpClient.PostAsJsonAsync("/api/containers", createRequest);

        // Act
        var response = await _httpClient.GetAsync("/api/containers");
        var containers = await response.Content.ReadFromJsonAsync<List<ContainerResponse>>();

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        containers.ShouldNotBeNull();
        containers.ShouldContain(c => c.Name == containerName);
    }

    private static ILocatorAssertions Expect(ILocator locator) => Assertions.Expect(locator);
}
