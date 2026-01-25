using AcceptanceTests.Infrastructure;
using Bogus;
using Microsoft.Playwright;

namespace AcceptanceTests;

[TestFixture]
public class AddContainerTests
{
    private PlaywrightServerFixture _serverFixture = null!;
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private string _baseUrl = null!;
    private Faker _faker = null!;

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

        _faker = new Faker();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _browser.DisposeAsync();
        _playwright.Dispose();

        await _serverFixture.DisposeAsync();
    }

    [Test]
    public async Task AddContainerButton_WhenClicked_ShouldOpenModal()
    {
        // Arrange
        var page = await _browser.NewPageAsync();
        await page.GotoAsync(_baseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Wait for spinner to disappear
        var spinner = page.Locator(".spinner-border");
        await spinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 30000 });

        // Act
        var addButton = page.Locator("button:has-text('Add Container')");
        await addButton.ClickAsync();

        // Assert
        var modal = page.Locator("#addContainerModal");
        await Expect(modal).ToBeVisibleAsync(new() { Timeout = 10000 });

        var modalTitle = page.Locator("#addContainerModalLabel");
        await Expect(modalTitle).ToContainTextAsync("Add Container");

        await page.CloseAsync();
    }

    [Test]
    public async Task AddContainerModal_ShouldDisplayNameInput()
    {
        // Arrange
        var page = await _browser.NewPageAsync();
        await page.GotoAsync(_baseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Wait for spinner to disappear
        var spinner = page.Locator(".spinner-border");
        await spinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 30000 });

        // Open the modal
        var addButton = page.Locator("button:has-text('Add Container')");
        await addButton.ClickAsync();

        // Wait for modal to be visible
        var modal = page.Locator("#addContainerModal");
        await Expect(modal).ToBeVisibleAsync(new() { Timeout = 10000 });

        // Assert
        var nameInput = page.Locator("#containerName");
        await Expect(nameInput).ToBeVisibleAsync(new() { Timeout = 5000 });

        var nameLabel = page.Locator("label[for='containerName']");
        await Expect(nameLabel).ToContainTextAsync("Name");

        await page.CloseAsync();
    }

    [Test]
    public async Task AddContainerModal_WithValidName_ShouldCreateContainerAndClose()
    {
        // Arrange
        var page = await _browser.NewPageAsync();
        await page.GotoAsync(_baseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Wait for spinner to disappear
        var spinner = page.Locator(".spinner-border");
        await spinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 30000 });

        // Open the modal
        var addButton = page.Locator("button:has-text('Add Container')");
        await addButton.ClickAsync();

        // Wait for modal to be visible
        var modal = page.Locator("#addContainerModal");
        await Expect(modal).ToBeVisibleAsync(new() { Timeout = 10000 });

        // Act - Enter a valid container name and submit
        var containerName = _faker.Commerce.ProductName();
        var nameInput = page.Locator("#containerName");
        await nameInput.FillAsync(containerName);

        var createButton = page.Locator(".modal-footer button.btn-primary:has-text('Create')");
        await createButton.ClickAsync();

        // Assert - Modal should close
        await Expect(modal).ToBeHiddenAsync(new() { Timeout = 15000 });

        await page.CloseAsync();
    }

    [Test]
    public async Task AddContainerModal_WithEmptyName_ShouldDisplayError()
    {
        // Arrange
        var page = await _browser.NewPageAsync();
        await page.GotoAsync(_baseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Wait for spinner to disappear
        var spinner = page.Locator(".spinner-border");
        await spinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 30000 });

        // Open the modal
        var addButton = page.Locator("button:has-text('Add Container')");
        await addButton.ClickAsync();

        // Wait for modal to be visible
        var modal = page.Locator("#addContainerModal");
        await Expect(modal).ToBeVisibleAsync(new() { Timeout = 10000 });

        // Act - Leave name empty and click create
        var createButton = page.Locator(".modal-footer button.btn-primary:has-text('Create')");
        await createButton.ClickAsync();

        // Assert - Should display validation error
        var errorMessage = page.Locator(".invalid-feedback, .alert-danger");
        await Expect(errorMessage).ToBeVisibleAsync(new() { Timeout = 10000 });

        // Modal should still be open
        await Expect(modal).ToBeVisibleAsync();

        await page.CloseAsync();
    }

    [Test]
    public async Task AddContainerModal_AfterSuccess_ShouldShowNewContainerInList()
    {
        // Arrange
        var page = await _browser.NewPageAsync();
        await page.GotoAsync(_baseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Wait for spinner to disappear
        var spinner = page.Locator(".spinner-border");
        await spinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 30000 });

        // Open the modal
        var addButton = page.Locator("button:has-text('Add Container')");
        await addButton.ClickAsync();

        // Wait for modal to be visible
        var modal = page.Locator("#addContainerModal");
        await Expect(modal).ToBeVisibleAsync(new() { Timeout = 10000 });

        // Act - Enter a unique container name and submit
        var containerName = $"Test Container {Guid.NewGuid():N}";
        var nameInput = page.Locator("#containerName");
        await nameInput.FillAsync(containerName);

        var createButton = page.Locator(".modal-footer button.btn-primary:has-text('Create')");
        await createButton.ClickAsync();

        // Wait for modal to close
        await Expect(modal).ToBeHiddenAsync(new() { Timeout = 15000 });

        // Assert - Container should appear in the table
        var containerRow = page.Locator($"td:has-text('{containerName}')");
        await Expect(containerRow).ToBeVisibleAsync(new() { Timeout = 10000 });

        await page.CloseAsync();
    }

    private static ILocatorAssertions Expect(ILocator locator) => Assertions.Expect(locator);
}
