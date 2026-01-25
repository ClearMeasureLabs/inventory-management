using AcceptanceTests.Infrastructure;
using Microsoft.Playwright;

namespace AcceptanceTests;

[TestFixture]
[Order(2)] // Run after ViewContainersTests since this creates containers
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

        // Act - Fill form fields (using Static SSR form - no SignalR needed)
        await page.FillAsync("#name", containerName);
        await page.FillAsync("#description", "Test Description");

        // Submit form - this triggers a traditional HTTP POST with page navigation
        var submitButton = page.Locator("button[type='submit']:has-text('Create Container')");
        await submitButton.ClickAsync();

        // Wait for navigation to complete (form POST redirects to home page)
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - should be on home page (not on /containers/add anymore)
        await Assertions.Expect(page).Not.ToHaveURLAsync(
            new System.Text.RegularExpressions.Regex(@".*/containers/add"), 
            new() { Timeout = 30000 });

        // Wait for spinner to disappear (indicating data has loaded)
        var spinner = page.Locator(".spinner-border");
        await spinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 30000 });

        // Verify the container is displayed in the list
        var containerRow = page.Locator($"td:has-text('{containerName}')");
        await Expect(containerRow).ToBeVisibleAsync(new() { Timeout = 10000 });

        await page.CloseAsync();
    }

    [Test]
    public async Task AddContainerPage_WhenEmptyFormSubmitted_ShouldDisplayValidationErrors()
    {
        // Arrange
        var page = await _browser.NewPageAsync();
        await page.GotoAsync($"{_baseUrl}/containers/add");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Act - Submit form without filling any fields
        var submitButton = page.Locator("button[type='submit']:has-text('Create Container')");
        await submitButton.ClickAsync();

        // Wait for page to process (form post and return with validation errors)
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - Should still be on the add container page
        await Assertions.Expect(page).ToHaveURLAsync(
            new System.Text.RegularExpressions.Regex(@".*/containers/add"),
            new() { Timeout = 10000 });

        // Validation errors should be displayed
        var validationSummary = page.Locator(".validation-summary-errors, .text-danger");
        await Expect(validationSummary.First).ToBeVisibleAsync(new() { Timeout = 10000 });

        // Check that "required" error messages are shown
        var pageContent = await page.ContentAsync();
        Assert.That(pageContent, Does.Contain("required").IgnoreCase, 
            "Expected validation error messages to mention 'required'");

        await page.CloseAsync();
    }

    [Test]
    public async Task AddContainerPage_WhenOnlyNameProvided_ShouldDisplayDescriptionValidationError()
    {
        // Arrange
        var page = await _browser.NewPageAsync();
        await page.GotoAsync($"{_baseUrl}/containers/add");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Act - Fill only the name field
        await page.FillAsync("#name", "Test Container");

        var submitButton = page.Locator("button[type='submit']:has-text('Create Container')");
        await submitButton.ClickAsync();

        // Wait for page to process
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - Should still be on the add container page with validation error
        await Assertions.Expect(page).ToHaveURLAsync(
            new System.Text.RegularExpressions.Regex(@".*/containers/add"),
            new() { Timeout = 10000 });

        // Check that description validation error is shown
        var pageContent = await page.ContentAsync();
        Assert.That(pageContent, Does.Contain("Description").IgnoreCase.And.Contain("required").IgnoreCase,
            "Expected validation error message for Description field");

        await page.CloseAsync();
    }

    private static ILocatorAssertions Expect(ILocator locator) => Assertions.Expect(locator);
}
