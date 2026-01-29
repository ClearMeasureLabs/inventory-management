using AcceptanceTests.Infrastructure;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace AcceptanceTests;

/// <summary>
/// Acceptance tests for breadcrumb navigation functionality.
/// </summary>
[TestFixture]
public class BreadcrumbNavigationTests : PageTest
{
    private int _testContainerId;
    private const string TestContainerName = "Test Container";

    [SetUp]
    public async Task SetUp()
    {
        // Create a test container via API
        var createResponse = await Page.APIRequest.PostAsync($"{TestEnvironment.WebApiUrl}/api/containers", new()
        {
            DataObject = new
            {
                name = TestContainerName,
                description = "Test Description"
            }
        });

        createResponse.Ok.ShouldBeTrue("Failed to create test container");

        var responseBody = await createResponse.JsonAsync();
        _testContainerId = responseBody?.GetProperty("containerId").GetInt32() ?? 0;
        _testContainerId.ShouldBeGreaterThan(0, "Container ID should be set");
    }

    [TearDown]
    public async Task TearDown()
    {
        // Clean up test container
        if (_testContainerId > 0)
        {
            await Page.APIRequest.DeleteAsync($"{TestEnvironment.WebApiUrl}/api/containers/{_testContainerId}");
        }
    }

    [Test]
    public async Task BreadcrumbDisplaysOnHomePage()
    {
        // Arrange & Act
        await Page.GotoAsync(TestEnvironment.WebAppUrl);

        // Wait for containers table to load (or empty state)
        await Task.Delay(500);

        // Assert
        var breadcrumbNav = Page.Locator("nav[aria-label='breadcrumb']");
        await Expect(breadcrumbNav).ToBeVisibleAsync();

        var breadcrumbText = await breadcrumbNav.TextContentAsync();
        breadcrumbText.ShouldNotBeNull();
        breadcrumbText.ShouldContain("Containers");

        // "Containers" should not be a link on home page
        var containersLink = breadcrumbNav.Locator("a", new() { HasText = "Containers" });
        await Expect(containersLink).ToHaveCountAsync(0);
    }

    [Test]
    public async Task BreadcrumbDisplaysOnContainerDetailsPage()
    {
        // Arrange & Act
        await Page.GotoAsync($"{TestEnvironment.WebAppUrl}/containers/{_testContainerId}");

        // Wait for container details to load
        await Expect(Page.Locator("h1")).ToContainTextAsync(TestContainerName);

        // Assert
        var breadcrumbNav = Page.Locator("nav[aria-label='breadcrumb']");
        await Expect(breadcrumbNav).ToBeVisibleAsync();

        var breadcrumbText = await breadcrumbNav.TextContentAsync();
        breadcrumbText.ShouldNotBeNull();
        breadcrumbText.ShouldContain("Containers");
        // Breadcrumb shows "Container [id]" format since container name isn't in route data
        breadcrumbText.ShouldContain($"Container {_testContainerId}");

        // "Containers" should be a clickable link
        var containersLink = breadcrumbNav.Locator("a", new() { HasText = "Containers" });
        await Expect(containersLink).ToHaveCountAsync(1);

        // Container name should not be a link (current page)
        var containerNameLink = breadcrumbNav.Locator($"a:has-text('{TestContainerName}')");
        await Expect(containerNameLink).ToHaveCountAsync(0);
    }

    [Test]
    public async Task NavigateHomeViaBreadcrumbLink()
    {
        // Arrange
        await Page.GotoAsync($"{TestEnvironment.WebAppUrl}/containers/{_testContainerId}");
        await Expect(Page.Locator("h1")).ToContainTextAsync(TestContainerName);

        // Act - Click "Containers" link in breadcrumb
        var breadcrumbNav = Page.Locator("nav[aria-label='breadcrumb']");
        var containersLink = breadcrumbNav.Locator("a", new() { HasText = "Containers" });
        await containersLink.ClickAsync();

        // Assert - Verify navigation to home page
        await Expect(Page).ToHaveURLAsync(TestEnvironment.WebAppUrl + "/");
        await Expect(Page.Locator("h1")).ToContainTextAsync("Containers");
    }

    [Test]
    public async Task BackToContainersLinkRemovedFromDetailsPage()
    {
        // Arrange & Act
        await Page.GotoAsync($"{TestEnvironment.WebAppUrl}/containers/{_testContainerId}");
        await Expect(Page.Locator("h1")).ToContainTextAsync(TestContainerName);

        // Assert - "Back to Containers" link should NOT be present (outside error alert)
        var backLinks = Page.Locator("a:has-text('Back to Containers')").Filter(new()
        {
            HasNot = Page.Locator(".alert-danger a")
        });
        await Expect(backLinks).ToHaveCountAsync(0);
    }

    [Test]
    public async Task ErrorStateRetainsRecoveryButton()
    {
        // Arrange & Act - Navigate to non-existent container
        await Page.GotoAsync($"{TestEnvironment.WebAppUrl}/containers/999999");

        // Assert - Error alert should be visible with recovery button
        await Expect(Page.Locator(".alert-danger")).ToBeVisibleAsync();
        await Expect(Page.Locator(".alert-danger")).ToContainTextAsync("Container not found");

        var recoveryButton = Page.Locator(".alert-danger a", new() { HasText = "Back to Containers" });
        await Expect(recoveryButton).ToBeVisibleAsync();
    }

    [Test]
    public async Task BreadcrumbHasProperStylingAndSpacing()
    {
        // Arrange & Act
        await Page.GotoAsync($"{TestEnvironment.WebAppUrl}/containers/{_testContainerId}");
        await Expect(Page.Locator("h1")).ToContainTextAsync(TestContainerName);

        // Assert - Breadcrumb nav should have proper styling
        var breadcrumbNav = Page.Locator("nav[aria-label='breadcrumb']");
        await Expect(breadcrumbNav).ToBeVisibleAsync();

        // Check that breadcrumb items exist with Bootstrap classes
        var breadcrumbItems = Page.Locator(".breadcrumb-item");
        await Expect(breadcrumbItems).ToHaveCountAsync(2);

        // Check for active class on current page
        var activeItem = Page.Locator(".breadcrumb-item.active");
        await Expect(activeItem).ToHaveCountAsync(1);
        await Expect(activeItem).ToHaveAttributeAsync("aria-current", "page");
    }
}
