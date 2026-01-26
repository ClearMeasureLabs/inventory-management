using System.Text.RegularExpressions;
using AcceptanceTests.Infrastructure;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace AcceptanceTests;

/// <summary>
/// Acceptance tests for Work Order management functionality.
/// Tests the full user workflow for creating, viewing, and deleting work orders.
/// </summary>
[TestFixture]
public class WorkOrderManagementTests : PageTest
{
    [Test]
    public async Task WorkOrders_Navigation_ShowsWorkOrdersPage()
    {
        // Arrange - Go to home page
        await Page.GotoAsync(TestEnvironment.WebAppUrl);

        // Act - Click on Work Orders in navigation
        await Page.ClickAsync("text=Work Orders");

        // Assert - Verify we're on the work orders page
        await Expect(Page).ToHaveURLAsync(new Regex("/work-orders"));
    }

    [Test]
    public async Task WorkOrders_CreateWorkOrder_AppearsInList()
    {
        // Arrange - Navigate to work orders page
        await Page.GotoAsync($"{TestEnvironment.WebAppUrl}/work-orders");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var uniqueTitle = $"Test Work Order {Guid.NewGuid():N}";

        // Act - Click Add Work Order button
        await Page.ClickAsync("button:has-text('Add Work Order')");

        // Wait for modal to appear
        await Expect(Page.Locator("#addWorkOrderModal")).ToBeVisibleAsync();

        // Fill in the title
        await Page.FillAsync("#workOrderTitle", uniqueTitle);

        // Click Create button
        await Page.ClickAsync("#addWorkOrderModal button:has-text('Create')");

        // Wait for modal to close and page to reload
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - Verify the work order appears in the table
        await Expect(Page.Locator($"text={uniqueTitle}")).ToBeVisibleAsync();
    }

    [Test]
    public async Task WorkOrders_DeleteWorkOrder_RemovedFromList()
    {
        // Arrange - Navigate to work orders page and create a work order
        await Page.GotoAsync($"{TestEnvironment.WebAppUrl}/work-orders");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var uniqueTitle = $"Delete Test {Guid.NewGuid():N}";

        // Create a work order first
        await Page.ClickAsync("button:has-text('Add Work Order')");
        await Expect(Page.Locator("#addWorkOrderModal")).ToBeVisibleAsync();
        await Page.FillAsync("#workOrderTitle", uniqueTitle);
        await Page.ClickAsync("#addWorkOrderModal button:has-text('Create')");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify the work order exists
        await Expect(Page.Locator($"text={uniqueTitle}")).ToBeVisibleAsync();

        // Act - Find the row with our work order and click its delete button
        var workOrderRow = Page.Locator($"tr:has-text('{uniqueTitle}')");
        await workOrderRow.Locator("button.btn-outline-danger").ClickAsync();

        // Wait for delete confirmation modal
        await Expect(Page.Locator("#deleteWorkOrderModal")).ToBeVisibleAsync();

        // Click Delete button in modal
        await Page.ClickAsync("#deleteWorkOrderModal button:has-text('Delete')");

        // Wait for page to reload
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - Verify the work order is no longer in the list
        await Expect(Page.Locator($"text={uniqueTitle}")).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task WorkOrders_EmptyState_ShowsAddButton()
    {
        // Arrange & Act - Navigate to work orders page
        await Page.GotoAsync($"{TestEnvironment.WebAppUrl}/work-orders");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - Verify Add Work Order button exists (either in empty state or table view)
        await Expect(Page.Locator("button:has-text('Add Work Order')")).ToBeVisibleAsync();
    }

    [Test]
    public async Task WorkOrders_PageTitle_ContainsWorkOrders()
    {
        // Arrange & Act
        await Page.GotoAsync($"{TestEnvironment.WebAppUrl}/work-orders");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert
        var title = await Page.TitleAsync();
        title.ShouldContain("Work Orders");
    }
}
