using AcceptanceTests.Infrastructure;
using Microsoft.Playwright;

namespace AcceptanceTests;

[TestFixture]
public class AddContainerTests
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
    public async Task AddContainerPage_ShouldNavigateFromHomePage()
    {
        // Arrange
        var page = await _browser.NewPageAsync();
        await page.GotoAsync(_baseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Wait for spinner to disappear
        var spinner = page.Locator(".spinner-border");
        await spinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 30000 });

        // Act
        var addButton = page.Locator("a:has-text('Add Container')");
        await addButton.ClickAsync();
        await page.WaitForURLAsync("**/containers/add");

        // Assert
        await Assertions.Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*/containers/add"));

        await page.CloseAsync();
    }

    [Test]
    public async Task AddContainerPage_ShouldDisplayFormFields()
    {
        // Arrange
        var page = await _browser.NewPageAsync();

        // Act
        await page.GotoAsync($"{_baseUrl}/containers/add");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        var heading = page.Locator("h1:has-text('Add Container')");
        await Expect(heading).ToBeVisibleAsync(new() { Timeout = 10000 });

        var nameInput = page.Locator("#name");
        await Expect(nameInput).ToBeVisibleAsync(new() { Timeout = 10000 });

        var descriptionInput = page.Locator("#description");
        await Expect(descriptionInput).ToBeVisibleAsync(new() { Timeout = 10000 });

        var submitButton = page.Locator("button[type='submit']:has-text('Create Container')");
        await Expect(submitButton).ToBeVisibleAsync(new() { Timeout = 10000 });

        var cancelButton = page.Locator("a:has-text('Cancel')");
        await Expect(cancelButton).ToBeVisibleAsync(new() { Timeout = 10000 });

        await page.CloseAsync();
    }

    [Test]
    public async Task AddContainerPage_CancelButton_ShouldNavigateToHomePage()
    {
        // Arrange
        var page = await _browser.NewPageAsync();
        await page.GotoAsync($"{_baseUrl}/containers/add");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Act
        var cancelButton = page.Locator("a:has-text('Cancel')");
        await cancelButton.ClickAsync();

        // Assert
        await Assertions.Expect(page).ToHaveURLAsync(_baseUrl + "/");

        await page.CloseAsync();
    }

    [Test]
    public async Task AddContainerPage_WhenValidDataSubmitted_ShouldCreateContainerAndRedirect()
    {
        // Arrange
        var page = await _browser.NewPageAsync();
        await page.GotoAsync($"{_baseUrl}/containers/add");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var containerName = $"Test Container {Guid.NewGuid():N}";

        // Act
        await page.FillAsync("#name", containerName);
        await page.FillAsync("#description", "Test Description");

        var submitButton = page.Locator("button[type='submit']:has-text('Create Container')");
        await submitButton.ClickAsync();

        // Wait for redirect
        await page.WaitForURLAsync(_baseUrl + "/", new() { Timeout = 30000 });

        // Assert - should be on home page
        await Assertions.Expect(page).ToHaveURLAsync(_baseUrl + "/");

        // Wait for the page to load and show the container
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Wait for spinner to disappear
        var spinner = page.Locator(".spinner-border");
        await spinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 30000 });

        // Verify the container is displayed in the list
        var containerRow = page.Locator($"td:has-text('{containerName}')");
        await Expect(containerRow).ToBeVisibleAsync(new() { Timeout = 10000 });

        await page.CloseAsync();
    }

    private static ILocatorAssertions Expect(ILocator locator) => Assertions.Expect(locator);
}
