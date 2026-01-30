using System.Text.RegularExpressions;
using AcceptanceTests.Infrastructure;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace AcceptanceTests;

/// <summary>
/// Acceptance tests for container details page functionality.
/// </summary>
[TestFixture]
public class ContainerDetailsTests : PageTest
{
    private int _testContainerId;

    [SetUp]
    public async Task SetUp()
    {
        // Create a test container via API
        var createResponse = await Page.APIRequest.PostAsync($"{TestEnvironment.WebApiUrl}/api/containers", new()
        {
            DataObject = new
            {
                name = "Test Container",
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
    public async Task NavigateToContainerDetails_FromContainersList_DisplaysContainerDetails()
    {
        // Arrange
        await Page.GotoAsync(TestEnvironment.WebAppUrl);

        // Wait for containers table to load
        await Expect(Page.Locator("table.table-striped")).ToBeVisibleAsync();

        // Act - Click on container ID link
        var containerLink = Page.Locator($"a[href='/containers/{_testContainerId}']").First;
        await containerLink.ClickAsync();

        // Assert - Verify we're on the details page
        await Expect(Page).ToHaveURLAsync(new Regex($"/containers/{_testContainerId}$"));

        // Verify container name is displayed as h1
        var heading = Page.Locator("h1");
        await Expect(heading).ToContainTextAsync("Test Container");

        // Verify description is displayed
        await Expect(Page.Locator("p.text-muted")).ToContainTextAsync("Test Description");
    }

    [Test]
    public async Task ContainerDetailsPage_WhenNoItems_DisplaysEmptyState()
    {
        // Arrange
        await Page.GotoAsync($"{TestEnvironment.WebAppUrl}/containers/{_testContainerId}");

        // Wait for page to load
        await Expect(Page.Locator("h1")).ToContainTextAsync("Test Container");

        // Assert - Verify items table is visible
        await Expect(Page.Locator("table.table-striped")).ToBeVisibleAsync();

        // Verify table headers
        await Expect(Page.Locator("th").Filter(new() { HasText = "Item ID" })).ToBeVisibleAsync();
        await Expect(Page.Locator("th").Filter(new() { HasText = "Name" })).ToBeVisibleAsync();

        // Verify empty state message
        await Expect(Page.Locator("td").Filter(new() { HasText = "No items in this container" })).ToBeVisibleAsync();
    }

    [Test]
    public async Task ContainerDetailsPage_BreadcrumbNavigation_NavigatesToContainersList()
    {
        // Arrange
        await Page.GotoAsync($"{TestEnvironment.WebAppUrl}/containers/{_testContainerId}");
        await Expect(Page.Locator("h1")).ToContainTextAsync("Test Container");

        // Act - Click "Containers" link in breadcrumb
        var breadcrumbNav = Page.Locator("nav[aria-label='breadcrumb']");
        var containersLink = breadcrumbNav.Locator("a", new() { HasText = "Containers" });
        await containersLink.ClickAsync();

        // Assert - Verify we're back on the home page
        await Expect(Page).ToHaveURLAsync(new Regex("^" + Regex.Escape(TestEnvironment.WebAppUrl) + "/?$"));
        await Expect(Page.Locator("h1")).ToContainTextAsync("Containers");
    }

    [Test]
    public async Task ContainerDetailsPage_NonExistentContainer_DisplaysNotFound()
    {
        // Arrange
        var nonExistentId = 999999;

        // Act
        await Page.GotoAsync($"{TestEnvironment.WebAppUrl}/containers/{nonExistentId}");

        // Assert
        await Expect(Page.Locator(".alert-danger")).ToBeVisibleAsync();
        await Expect(Page.Locator(".alert-danger")).ToContainTextAsync("Container not found");

        // Verify back link is present
        var backLink = Page.Locator(".alert-danger a").Filter(new() { HasText = "Back to Containers" });
        await Expect(backLink).ToBeVisibleAsync();
    }

    [Test]
    public async Task EditButton_IsVisible_OnDetailsPage()
    {
        // Arrange & Act
        await Page.GotoAsync($"{TestEnvironment.WebAppUrl}/containers/{_testContainerId}");
        await Expect(Page.Locator("h1")).ToContainTextAsync("Test Container");

        // Assert
        var editButton = Page.Locator("button[aria-label='Edit container']");
        await Expect(editButton).ToBeVisibleAsync();
        await Expect(editButton).ToContainTextAsync("Edit");
    }

    [Test]
    public async Task DeleteButton_IsVisible_OnDetailsPage()
    {
        // Arrange & Act
        await Page.GotoAsync($"{TestEnvironment.WebAppUrl}/containers/{_testContainerId}");
        await Expect(Page.Locator("h1")).ToContainTextAsync("Test Container");

        // Assert
        var deleteButton = Page.Locator("button[aria-label='Delete container']");
        await Expect(deleteButton).ToBeVisibleAsync();
        await Expect(deleteButton).ToContainTextAsync("Delete");
    }

    [Test]
    public async Task EditContainer_InlineForm_SavesChanges()
    {
        // Arrange
        await Page.GotoAsync($"{TestEnvironment.WebAppUrl}/containers/{_testContainerId}");
        await Expect(Page.Locator("h1")).ToContainTextAsync("Test Container");

        // Act - Click Edit button
        var editButton = Page.Locator("button[aria-label='Edit container']");
        await editButton.ClickAsync();

        // Verify edit mode is active
        await Expect(Page.Locator("input.form-control-lg")).ToBeVisibleAsync();
        await Expect(Page.Locator("textarea")).ToBeVisibleAsync();

        // Update name and description
        await Page.Locator("input.form-control-lg").FillAsync("Updated Container Name");
        await Page.Locator("textarea").FillAsync("Updated description text");

        // Click Save button
        var saveButton = Page.Locator("button[aria-label='Save changes']");
        await saveButton.ClickAsync();

        // Assert - Verify changes are saved and view mode is restored
        await Expect(Page.Locator("h1")).ToContainTextAsync("Updated Container Name");
        await Expect(Page.Locator("p.text-muted")).ToContainTextAsync("Updated description text");

        // Verify breadcrumb updated
        await Expect(Page.Locator("nav[aria-label='breadcrumb']")).ToContainTextAsync("Updated Container Name");

        // Verify edit/delete buttons are visible again
        await Expect(Page.Locator("button[aria-label='Edit container']")).ToBeVisibleAsync();
        await Expect(Page.Locator("button[aria-label='Delete container']")).ToBeVisibleAsync();
    }

    [Test]
    public async Task EditContainer_InlineForm_CancelsChanges()
    {
        // Arrange
        await Page.GotoAsync($"{TestEnvironment.WebAppUrl}/containers/{_testContainerId}");
        await Expect(Page.Locator("h1")).ToContainTextAsync("Test Container");

        // Act - Click Edit button
        var editButton = Page.Locator("button[aria-label='Edit container']");
        await editButton.ClickAsync();

        // Modify fields
        await Page.Locator("input.form-control-lg").FillAsync("Modified Name");
        await Page.Locator("textarea").FillAsync("Modified description");

        // Click Cancel button
        var cancelButton = Page.Locator("button[aria-label='Cancel editing']");
        await cancelButton.ClickAsync();

        // Assert - Verify original values are still displayed
        await Expect(Page.Locator("h1")).ToContainTextAsync("Test Container");
        await Expect(Page.Locator("p.text-muted")).ToContainTextAsync("Test Description");

        // Verify edit/delete buttons are visible
        await Expect(Page.Locator("button[aria-label='Edit container']")).ToBeVisibleAsync();
        await Expect(Page.Locator("button[aria-label='Delete container']")).ToBeVisibleAsync();
    }

    [Test]
    public async Task DeleteContainer_Modal_DeletesAndNavigates()
    {
        // Arrange
        await Page.GotoAsync($"{TestEnvironment.WebAppUrl}/containers/{_testContainerId}");
        await Expect(Page.Locator("h1")).ToContainTextAsync("Test Container");

        // Act - Click Delete button
        var deleteButton = Page.Locator("button[aria-label='Delete container']");
        await deleteButton.ClickAsync();

        // Verify modal is visible
        await Expect(Page.Locator("#deleteContainerModal")).ToBeVisibleAsync();
        await Expect(Page.Locator("#deleteContainerModal")).ToContainTextAsync("Delete Container");
        await Expect(Page.Locator("#deleteContainerModal")).ToContainTextAsync("Test Container");

        // Click Delete in modal
        var confirmButton = Page.Locator("#deleteContainerModal button.btn-danger");
        await confirmButton.ClickAsync();

        // Assert - Verify navigation to home page
        await Expect(Page).ToHaveURLAsync(new Regex("^" + Regex.Escape(TestEnvironment.WebAppUrl) + "/?$"));
        await Expect(Page.Locator("h1")).ToContainTextAsync("Containers");

        // Mark container as deleted so TearDown doesn't try to delete it again
        _testContainerId = 0;
    }

    [Test]
    public async Task DeleteContainer_Modal_CancelsDelete()
    {
        // Arrange
        await Page.GotoAsync($"{TestEnvironment.WebAppUrl}/containers/{_testContainerId}");
        await Expect(Page.Locator("h1")).ToContainTextAsync("Test Container");

        // Act - Click Delete button
        var deleteButton = Page.Locator("button[aria-label='Delete container']");
        await deleteButton.ClickAsync();

        // Verify modal is visible
        await Expect(Page.Locator("#deleteContainerModal")).ToBeVisibleAsync();

        // Click Cancel in modal
        var cancelButton = Page.Locator("#deleteContainerModal button.btn-secondary");
        await cancelButton.ClickAsync();

        // Assert - Verify modal is closed and we're still on the details page
        await Expect(Page.Locator("#deleteContainerModal")).Not.ToBeVisibleAsync();
        await Expect(Page.Locator("h1")).ToContainTextAsync("Test Container");
        await Expect(Page).ToHaveURLAsync(new Regex($"/containers/{_testContainerId}$"));
    }

    [Test]
    public async Task EditDeleteButtons_NotVisible_WhenContainerNotFound()
    {
        // Arrange
        var nonExistentId = 999999;

        // Act
        await Page.GotoAsync($"{TestEnvironment.WebAppUrl}/containers/{nonExistentId}");

        // Wait for error to display
        await Expect(Page.Locator(".alert-danger")).ToBeVisibleAsync();

        // Assert - Verify edit/delete buttons are not present
        var editButton = Page.Locator("button[aria-label='Edit container']");
        var deleteButton = Page.Locator("button[aria-label='Delete container']");

        await Expect(editButton).Not.ToBeVisibleAsync();
        await Expect(deleteButton).Not.ToBeVisibleAsync();
    }
}
