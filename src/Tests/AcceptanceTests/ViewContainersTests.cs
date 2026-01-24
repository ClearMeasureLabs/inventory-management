using AcceptanceTests.Infrastructure;
using Microsoft.Playwright;

namespace AcceptanceTests;

[TestFixture]
public class ViewContainersTests
{
    private TestEnvironment _testEnvironment = null!;
    private PlaywrightServerFixture _serverFixture = null!;
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private string _baseUrl = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        // Initialize test containers
        _testEnvironment = new TestEnvironment();
        await _testEnvironment.InitializeAsync();

        // Create and start the Kestrel server for Playwright tests
        _serverFixture = new PlaywrightServerFixture(_testEnvironment);
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
        await _testEnvironment.DisposeAsync();
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

        // Assert
        var title = await page.TitleAsync();
        Assert.That(title, Is.EqualTo("Ivan"));

        await page.CloseAsync();
    }

    [Test]
    public async Task LandingPage_WhenNoContainers_ShouldDisplayEmptyStateJumbotron()
    {
        // Arrange
        var page = await _browser.NewPageAsync();

        // Act
        await page.GotoAsync(_baseUrl);
        
        // Wait for Blazor to hydrate and content to load
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        var jumbotron = page.Locator(".bg-light.rounded-3");
        await Expect(jumbotron).ToBeVisibleAsync();

        var heading = page.Locator("h1:has-text('No Containers')");
        await Expect(heading).ToBeVisibleAsync();

        var addButton = page.Locator("button:has-text('Add Container')");
        await Expect(addButton).ToBeVisibleAsync();

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

        // Assert
        var addButton = page.Locator("button.btn-primary:has-text('Add Container')");
        await Expect(addButton).ToBeVisibleAsync();

        await page.CloseAsync();
    }

    private static ILocatorAssertions Expect(ILocator locator) => Assertions.Expect(locator);
}
