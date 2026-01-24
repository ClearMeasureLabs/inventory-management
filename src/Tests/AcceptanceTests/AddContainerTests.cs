using Microsoft.Playwright;

namespace AcceptanceTests;

/// <summary>
/// Acceptance tests for Add Container functionality.
/// These tests run against a locally running Blazor app (not Testcontainers).
/// 
/// Prerequisites:
/// 1. Start infrastructure: cd environments/local && ./provision.ps1
/// 2. Run the app: dotnet run --project src/Presentation/WebApp
/// 3. Run these tests: dotnet test --filter "FullyQualifiedName~AddContainerTests"
/// </summary>
[TestFixture]
[Category("LocalApp")]
public class AddContainerTests
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private const string BaseUrl = "https://localhost:5001"; // Default HTTPS port

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
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
    }

    [Test]
    public async Task AddContainerButton_WhenClicked_ShouldOpenModal()
    {
        // Arrange
        var context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true
        });
        var page = await context.NewPageAsync();
        
        await page.GotoAsync(BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Wait for Blazor to fully initialize
        await page.WaitForTimeoutAsync(2000);

        // Act
        var addButton = page.Locator("button:has-text('Add Container')");
        await addButton.ClickAsync();

        // Assert
        var modal = page.Locator("#addContainerModal");
        await Expect(modal).ToBeVisibleAsync(new() { Timeout = 10000 });

        var modalTitle = page.Locator("#addContainerModalLabel");
        await Expect(modalTitle).ToContainTextAsync("Add Container");

        await page.CloseAsync();
        await context.CloseAsync();
    }

    [Test]
    public async Task AddContainerModal_ShouldDisplayNameInput()
    {
        // Arrange
        var context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true
        });
        var page = await context.NewPageAsync();
        
        await page.GotoAsync(BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.WaitForTimeoutAsync(2000);

        // Act
        var addButton = page.Locator("button:has-text('Add Container')");
        await addButton.ClickAsync();

        var modal = page.Locator("#addContainerModal");
        await Expect(modal).ToBeVisibleAsync(new() { Timeout = 10000 });

        // Assert
        var nameInput = page.Locator("#containerName");
        await Expect(nameInput).ToBeVisibleAsync();

        var nameLabel = page.Locator("label[for='containerName']");
        await Expect(nameLabel).ToContainTextAsync("Name");

        await page.CloseAsync();
        await context.CloseAsync();
    }

    [Test]
    public async Task AddContainerModal_WithValidName_ShouldCreateContainerAndClose()
    {
        // Arrange
        var context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true
        });
        var page = await context.NewPageAsync();
        
        await page.GotoAsync(BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.WaitForTimeoutAsync(2000);

        var containerName = $"Test Container {Guid.NewGuid():N}";

        // Act - Open modal and fill form
        var addButton = page.Locator("button:has-text('Add Container')");
        await addButton.ClickAsync();

        var modal = page.Locator("#addContainerModal");
        await Expect(modal).ToBeVisibleAsync(new() { Timeout = 10000 });

        var nameInput = page.Locator("#containerName");
        await nameInput.FillAsync(containerName);

        var createButton = page.Locator("button.btn-primary:has-text('Create')");
        await createButton.ClickAsync();

        // Wait for the container to appear in the list
        var containerRow = page.Locator($"td:has-text('{containerName}')");
        await Expect(containerRow).ToBeVisibleAsync(new() { Timeout = 15000 });

        // Assert - Modal should be hidden after success
        await Expect(modal).ToBeHiddenAsync(new() { Timeout = 5000 });

        await page.CloseAsync();
        await context.CloseAsync();
    }

    [Test]
    public async Task AddContainerModal_WithEmptyName_ShouldDisplayError()
    {
        // Arrange
        var context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true
        });
        var page = await context.NewPageAsync();
        
        await page.GotoAsync(BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.WaitForTimeoutAsync(2000);

        // Act - Open modal and submit without filling name
        var addButton = page.Locator("button:has-text('Add Container')");
        await addButton.ClickAsync();

        var modal = page.Locator("#addContainerModal");
        await Expect(modal).ToBeVisibleAsync(new() { Timeout = 10000 });

        var createButton = page.Locator("button.btn-primary:has-text('Create')");
        await createButton.ClickAsync();

        // Assert - Error message should be displayed
        var errorAlert = page.Locator(".alert-danger");
        await Expect(errorAlert).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(errorAlert).ToContainTextAsync("Name is required");

        await page.CloseAsync();
        await context.CloseAsync();
    }

    [Test]
    public async Task AddContainerModal_AfterSuccess_ShouldShowNewContainerInList()
    {
        // Arrange
        var context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true
        });
        var page = await context.NewPageAsync();
        
        await page.GotoAsync(BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.WaitForTimeoutAsync(2000);

        var containerName = $"New Container {Guid.NewGuid():N}";

        // Act - Create a container
        var addButton = page.Locator("button:has-text('Add Container')");
        await addButton.ClickAsync();

        var modal = page.Locator("#addContainerModal");
        await Expect(modal).ToBeVisibleAsync(new() { Timeout = 10000 });

        var nameInput = page.Locator("#containerName");
        await nameInput.FillAsync(containerName);

        var createButton = page.Locator("button.btn-primary:has-text('Create')");
        await createButton.ClickAsync();

        // Wait for the container cell to appear
        var containerCell = page.Locator($"td:has-text('{containerName}')");
        await Expect(containerCell).ToBeVisibleAsync(new() { Timeout = 15000 });

        // Assert - The table should now be visible with the new container
        var table = page.Locator("table.table-striped");
        await Expect(table).ToBeVisibleAsync();

        await page.CloseAsync();
        await context.CloseAsync();
    }

    private static ILocatorAssertions Expect(ILocator locator) => Assertions.Expect(locator);
}
