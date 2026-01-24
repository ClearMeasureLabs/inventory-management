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

    [Test]
    public async Task AddContainerButton_WhenClicked_ShouldOpenModal()
    {
        // Arrange
        var page = await _browser.NewPageAsync();
        await page.GotoAsync(_baseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Wait for Blazor SignalR connection to be ready
        await page.WaitForTimeoutAsync(1000);

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
        
        // Wait for Blazor SignalR connection to be ready
        await page.WaitForTimeoutAsync(1000);

        // Act
        var addButton = page.Locator("button:has-text('Add Container')");
        await addButton.ClickAsync();

        // Wait for modal to appear
        var modal = page.Locator("#addContainerModal");
        await Expect(modal).ToBeVisibleAsync(new() { Timeout = 10000 });

        // Assert
        var nameInput = page.Locator("#containerName");
        await Expect(nameInput).ToBeVisibleAsync();

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
        
        // Wait for Blazor SignalR connection to be ready
        await page.WaitForTimeoutAsync(1000);

        var containerName = $"Test Container {Guid.NewGuid():N}";

        // Act - Open modal and fill form
        var addButton = page.Locator("button:has-text('Add Container')");
        await addButton.ClickAsync();

        // Wait for modal to be visible
        var modal = page.Locator("#addContainerModal");
        await Expect(modal).ToBeVisibleAsync(new() { Timeout = 10000 });

        var nameInput = page.Locator("#containerName");
        await nameInput.FillAsync(containerName);

        var createButton = page.Locator("button.btn-primary:has-text('Create')");
        await createButton.ClickAsync();

        // Wait for the container to appear in the list (indicates success)
        var containerRow = page.Locator($"td:has-text('{containerName}')");
        await Expect(containerRow).ToBeVisibleAsync(new() { Timeout = 15000 });

        // Assert - Modal should be hidden after success
        await Expect(modal).ToBeHiddenAsync(new() { Timeout = 5000 });

        await page.CloseAsync();
    }

    [Test]
    public async Task AddContainerModal_WithEmptyName_ShouldDisplayError()
    {
        // Arrange
        var page = await _browser.NewPageAsync();
        await page.GotoAsync(_baseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Wait for Blazor SignalR connection to be ready
        await page.WaitForTimeoutAsync(1000);

        // Act - Open modal and submit without filling name
        var addButton = page.Locator("button:has-text('Add Container')");
        await addButton.ClickAsync();

        // Wait for modal to be visible
        var modal = page.Locator("#addContainerModal");
        await Expect(modal).ToBeVisibleAsync(new() { Timeout = 10000 });

        var createButton = page.Locator("button.btn-primary:has-text('Create')");
        await createButton.ClickAsync();

        // Assert - Error message should be displayed
        var errorAlert = page.Locator(".alert-danger");
        await Expect(errorAlert).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(errorAlert).ToContainTextAsync("Name is required");

        await page.CloseAsync();
    }

    [Test]
    public async Task AddContainerModal_AfterSuccess_ShouldShowNewContainerInList()
    {
        // Arrange
        var page = await _browser.NewPageAsync();
        await page.GotoAsync(_baseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Wait for Blazor SignalR connection to be ready
        await page.WaitForTimeoutAsync(1000);

        var containerName = $"New Container {Guid.NewGuid():N}";

        // Act - Create a container
        var addButton = page.Locator("button:has-text('Add Container')");
        await addButton.ClickAsync();

        // Wait for modal to be visible
        var modal = page.Locator("#addContainerModal");
        await Expect(modal).ToBeVisibleAsync(new() { Timeout = 10000 });

        var nameInput = page.Locator("#containerName");
        await nameInput.FillAsync(containerName);

        var createButton = page.Locator("button.btn-primary:has-text('Create')");
        await createButton.ClickAsync();

        // Wait for the container cell to appear (indicates success)
        var containerCell = page.Locator($"td:has-text('{containerName}')");
        await Expect(containerCell).ToBeVisibleAsync(new() { Timeout = 15000 });

        // Assert - The table should now be visible with the new container
        var table = page.Locator("table.table-striped");
        await Expect(table).ToBeVisibleAsync();

        await page.CloseAsync();
    }

    private static ILocatorAssertions Expect(ILocator locator) => Assertions.Expect(locator);
}
