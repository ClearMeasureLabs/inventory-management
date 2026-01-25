using AcceptanceTests.Infrastructure;
using Microsoft.Playwright;

namespace AcceptanceTests;

[TestFixture]
public class ViewContainersTests
{
    private PlaywrightServerFixture _serverFixture = null!;
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private string _baseUrl = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        // Use shared test environment from global fixture
        _serverFixture = new PlaywrightServerFixture(GlobalTestFixture.TestEnvironment);
        await _serverFixture.StartAsync();
        _baseUrl = _serverFixture.ServerAddress;

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

        await _serverFixture.DisposeAsync();
    }

    [Test]
    public async Task LandingPage_ShouldDisplayNavbarWithBranding()
    {
        // Arrange
        var page = await _browser.NewPageAsync();

        // Act
        await page.GotoAsync(_baseUrl);

        // Assert
        var navbar = page.Locator("nav.navbar");
        await Expect(navbar).ToBeVisibleAsync();

        var brand = page.Locator(".navbar-brand");
        await Expect(brand).ToContainTextAsync("Ivan | Inventory Management");

        await page.CloseAsync();
    }

    [Test]
    public async Task LandingPage_ShouldHaveCorrectTitle()
    {
        // Arrange
        var page = await _browser.NewPageAsync();

        // Act
        await page.GotoAsync(_baseUrl);
        
        // Wait for Blazor to hydrate
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - wait for title to be set by Blazor
        await Assertions.Expect(page).ToHaveTitleAsync("Ivan", new() { Timeout = 30000 });

        await page.CloseAsync();
    }

    [Test]
    public async Task LandingPage_ShouldDisplayContainersOrEmptyState()
    {
        // Arrange
        var page = await _browser.NewPageAsync();

        // Act
        await page.GotoAsync(_baseUrl);
        
        // Wait for Blazor to hydrate
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Wait for spinner to disappear (indicating Blazor Server has finished loading data)
        var spinner = page.Locator(".spinner-border");
        await spinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 30000 });

        // Assert - Either empty state jumbotron or containers table should be visible
        var jumbotron = page.Locator(".bg-light.rounded-3");
        var containersTable = page.Locator("table.table-striped");
        
        // Check if either the jumbotron (empty state) or table (with containers) is visible
        var isJumbotronVisible = await jumbotron.IsVisibleAsync();
        var isTableVisible = await containersTable.IsVisibleAsync();
        
        Assert.That(isJumbotronVisible || isTableVisible, Is.True, 
            "Either the empty state jumbotron or the containers table should be visible");

        // Add button should always be present
        var addButton = page.Locator("button:has-text('Add Container')");
        await Expect(addButton).ToBeVisibleAsync(new() { Timeout = 10000 });

        await page.CloseAsync();
    }


    [Test]
    public async Task LandingPage_WhenNoContainers_AddButtonShouldBePresent()
    {
        // Arrange
        var page = await _browser.NewPageAsync();

        // Act
        await page.GotoAsync(_baseUrl);
        
        // Wait for Blazor to hydrate
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Wait for spinner to disappear (indicating Blazor Server has finished loading data)
        var spinner = page.Locator(".spinner-border");
        await spinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 30000 });

        // Assert
        var addButton = page.Locator("button.btn-primary:has-text('Add Container')");
        await Expect(addButton).ToBeVisibleAsync(new() { Timeout = 10000 });

        await page.CloseAsync();
    }

    private static ILocatorAssertions Expect(ILocator locator) => Assertions.Expect(locator);
}
