using AcceptanceTests.Infrastructure;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace AcceptanceTests;

/// <summary>
/// Acceptance tests for container sorting and filtering functionality.
/// </summary>
[TestFixture]
public class ContainerSortFilterTests : PageTest
{
    private readonly List<int> _testContainerIds = new();

    [SetUp]
    public async Task SetUp()
    {
        // Create test containers via API with known names for predictable sorting/filtering
        var containers = new[]
        {
            new { name = "Alpha Container", description = "First test container" },
            new { name = "Beta Container", description = "Second test container" },
            new { name = "Gamma", description = "Third test container" }
        };

        foreach (var container in containers)
        {
            var createResponse = await Page.APIRequest.PostAsync($"{TestEnvironment.WebApiUrl}/api/containers", new()
            {
                DataObject = container
            });

            createResponse.Ok.ShouldBeTrue($"Failed to create test container: {container.name}");

            var responseBody = await createResponse.JsonAsync();
            var containerId = responseBody?.GetProperty("containerId").GetInt32() ?? 0;
            containerId.ShouldBeGreaterThan(0, "Container ID should be set");
            _testContainerIds.Add(containerId);
        }
    }

    [TearDown]
    public async Task TearDown()
    {
        // Clean up test containers
        foreach (var containerId in _testContainerIds)
        {
            await Page.APIRequest.DeleteAsync($"{TestEnvironment.WebApiUrl}/api/containers/{containerId}");
        }
        _testContainerIds.Clear();
    }

    #region Filter Tests

    [Test]
    public async Task FilterContainersByName_PartialMatch_ShowsMatchingContainers()
    {
        // Arrange
        await Page.GotoAsync(TestEnvironment.WebAppUrl);
        await Expect(Page.Locator("table.table-striped")).ToBeVisibleAsync();

        // Act - Type "Container" in search input
        var searchInput = Page.Locator("#containerSearch");
        await searchInput.FillAsync("Container");

        // Assert - Only containers with "Container" in name should be visible
        await Expect(Page.Locator("table tbody tr")).ToHaveCountAsync(2);
        await Expect(Page.Locator("table tbody")).ToContainTextAsync("Alpha Container");
        await Expect(Page.Locator("table tbody")).ToContainTextAsync("Beta Container");
        await Expect(Page.Locator("table tbody")).Not.ToContainTextAsync("Gamma");
    }

    [Test]
    public async Task FilterContainersByName_CaseInsensitive_ShowsMatchingContainers()
    {
        // Arrange
        await Page.GotoAsync(TestEnvironment.WebAppUrl);
        await Expect(Page.Locator("table.table-striped")).ToBeVisibleAsync();

        // Act - Type lowercase search term
        var searchInput = Page.Locator("#containerSearch");
        await searchInput.FillAsync("alpha");

        // Assert - Should match "Alpha Container" despite case difference
        await Expect(Page.Locator("table tbody tr")).ToHaveCountAsync(1);
        await Expect(Page.Locator("table tbody")).ToContainTextAsync("Alpha Container");
    }

    [Test]
    public async Task ClearSearchFilter_ShowsAllContainers()
    {
        // Arrange
        await Page.GotoAsync(TestEnvironment.WebAppUrl);
        await Expect(Page.Locator("table.table-striped")).ToBeVisibleAsync();

        // Apply filter first
        var searchInput = Page.Locator("#containerSearch");
        await searchInput.FillAsync("Alpha");
        await Expect(Page.Locator("table tbody tr")).ToHaveCountAsync(1);

        // Act - Click clear button
        var clearButton = Page.Locator("button[aria-label='Clear search']");
        await clearButton.ClickAsync();

        // Assert - All containers should be visible again
        await Expect(Page.Locator("table tbody tr")).ToHaveCountAsync(3);
    }

    [Test]
    public async Task FilterContainers_NoMatches_ShowsNoResultsMessage()
    {
        // Arrange
        await Page.GotoAsync(TestEnvironment.WebAppUrl);
        await Expect(Page.Locator("table.table-striped")).ToBeVisibleAsync();

        // Act - Type a search term that matches nothing
        var searchInput = Page.Locator("#containerSearch");
        await searchInput.FillAsync("Nonexistent");

        // Assert - Should show no results message
        await Expect(Page.Locator(".alert-info")).ToBeVisibleAsync();
        await Expect(Page.Locator(".alert-info")).ToContainTextAsync("No containers found matching");
    }

    #endregion

    #region Sort Tests

    [Test]
    public async Task SortContainersById_Ascending_SortsByIdAscending()
    {
        // Arrange
        await Page.GotoAsync(TestEnvironment.WebAppUrl);
        await Expect(Page.Locator("table.table-striped")).ToBeVisibleAsync();

        // Act - Click ID column header
        var idHeader = Page.Locator("th").Filter(new() { HasText = "ID" }).First;
        await idHeader.ClickAsync();

        // Assert - Should show ascending arrow and have aria-sort
        await Expect(idHeader).ToContainTextAsync("▲");
        await Expect(idHeader).ToHaveAttributeAsync("aria-sort", "ascending");

        // Verify order - first row should have smallest ID
        var firstRowId = await Page.Locator("table tbody tr").First.Locator("td").First.TextContentAsync();
        var lastRowId = await Page.Locator("table tbody tr").Last.Locator("td").First.TextContentAsync();
        
        int.Parse(firstRowId!).ShouldBeLessThan(int.Parse(lastRowId!), "First ID should be less than last ID");
    }

    [Test]
    public async Task SortContainersById_Descending_SortsByIdDescending()
    {
        // Arrange
        await Page.GotoAsync(TestEnvironment.WebAppUrl);
        await Expect(Page.Locator("table.table-striped")).ToBeVisibleAsync();

        // Act - Click ID header twice (first asc, then desc)
        var idHeader = Page.Locator("th").Filter(new() { HasText = "ID" }).First;
        await idHeader.ClickAsync();
        await idHeader.ClickAsync();

        // Assert - Should show descending arrow
        await Expect(idHeader).ToContainTextAsync("▼");
        await Expect(idHeader).ToHaveAttributeAsync("aria-sort", "descending");

        // Verify order - first row should have largest ID
        var firstRowId = await Page.Locator("table tbody tr").First.Locator("td").First.TextContentAsync();
        var lastRowId = await Page.Locator("table tbody tr").Last.Locator("td").First.TextContentAsync();
        
        int.Parse(firstRowId!).ShouldBeGreaterThan(int.Parse(lastRowId!), "First ID should be greater than last ID");
    }

    [Test]
    public async Task SortContainersByName_Ascending_SortsByNameAscending()
    {
        // Arrange
        await Page.GotoAsync(TestEnvironment.WebAppUrl);
        await Expect(Page.Locator("table.table-striped")).ToBeVisibleAsync();

        // Act - Click Name column header
        var nameHeader = Page.Locator("th").Filter(new() { HasText = "Name" }).First;
        await nameHeader.ClickAsync();

        // Assert - Should show ascending arrow
        await Expect(nameHeader).ToContainTextAsync("▲");
        await Expect(nameHeader).ToHaveAttributeAsync("aria-sort", "ascending");

        // Verify order - Alpha should be first, Gamma should be last
        var rows = Page.Locator("table tbody tr");
        var firstRowName = await rows.First.Locator("td").Nth(1).TextContentAsync();
        var lastRowName = await rows.Last.Locator("td").Nth(1).TextContentAsync();
        
        firstRowName.ShouldNotBeNull();
        lastRowName.ShouldNotBeNull();
        firstRowName.ShouldContain("Alpha");
        lastRowName.ShouldContain("Gamma");
    }

    [Test]
    public async Task SortContainersByName_Descending_SortsByNameDescending()
    {
        // Arrange
        await Page.GotoAsync(TestEnvironment.WebAppUrl);
        await Expect(Page.Locator("table.table-striped")).ToBeVisibleAsync();

        // Act - Click Name header twice
        var nameHeader = Page.Locator("th").Filter(new() { HasText = "Name" }).First;
        await nameHeader.ClickAsync();
        await nameHeader.ClickAsync();

        // Assert - Should show descending arrow
        await Expect(nameHeader).ToContainTextAsync("▼");
        await Expect(nameHeader).ToHaveAttributeAsync("aria-sort", "descending");

        // Verify order - Gamma should be first, Alpha should be last
        var rows = Page.Locator("table tbody tr");
        var firstRowName = await rows.First.Locator("td").Nth(1).TextContentAsync();
        var lastRowName = await rows.Last.Locator("td").Nth(1).TextContentAsync();
        
        firstRowName.ShouldNotBeNull();
        lastRowName.ShouldNotBeNull();
        firstRowName.ShouldContain("Gamma");
        lastRowName.ShouldContain("Alpha");
    }

    #endregion

    #region Combined Filter and Sort Tests

    [Test]
    public async Task FilterAndSort_Together_AppliesBothCorrectly()
    {
        // Arrange
        await Page.GotoAsync(TestEnvironment.WebAppUrl);
        await Expect(Page.Locator("table.table-striped")).ToBeVisibleAsync();

        // Act - Filter by "Container" then sort by ID ascending
        var searchInput = Page.Locator("#containerSearch");
        await searchInput.FillAsync("Container");

        var idHeader = Page.Locator("th").Filter(new() { HasText = "ID" }).First;
        await idHeader.ClickAsync();

        // Assert - Should show only filtered containers, sorted by ID
        await Expect(Page.Locator("table tbody tr")).ToHaveCountAsync(2);
        
        // Both rows should contain "Container"
        await Expect(Page.Locator("table tbody tr").First).ToContainTextAsync("Container");
        await Expect(Page.Locator("table tbody tr").Last).ToContainTextAsync("Container");

        // Should be in ID ascending order
        var firstRowId = await Page.Locator("table tbody tr").First.Locator("td").First.TextContentAsync();
        var lastRowId = await Page.Locator("table tbody tr").Last.Locator("td").First.TextContentAsync();
        
        int.Parse(firstRowId!).ShouldBeLessThan(int.Parse(lastRowId!), "Filtered results should be sorted by ID ascending");
    }

    #endregion

    #region Accessibility Tests

    [Test]
    public async Task SortHeaders_AreKeyboardAccessible()
    {
        // Arrange
        await Page.GotoAsync(TestEnvironment.WebAppUrl);
        await Expect(Page.Locator("table.table-striped")).ToBeVisibleAsync();

        // Act - Tab to ID header and press Enter
        var idHeader = Page.Locator("th").Filter(new() { HasText = "ID" }).First;
        await idHeader.FocusAsync();
        await Page.Keyboard.PressAsync("Enter");

        // Assert - Should sort by ID
        await Expect(idHeader).ToContainTextAsync("▲");
        await Expect(idHeader).ToHaveAttributeAsync("aria-sort", "ascending");

        // Press Space to toggle
        await Page.Keyboard.PressAsync("Space");
        await Expect(idHeader).ToContainTextAsync("▼");
        await Expect(idHeader).ToHaveAttributeAsync("aria-sort", "descending");
    }

    [Test]
    public async Task SearchInput_HasProperAccessibilityAttributes()
    {
        // Arrange
        await Page.GotoAsync(TestEnvironment.WebAppUrl);
        await Expect(Page.Locator("table.table-striped")).ToBeVisibleAsync();

        // Assert - Search input should have proper aria-label
        var searchInput = Page.Locator("#containerSearch");
        await Expect(searchInput).ToHaveAttributeAsync("aria-label", "Filter containers by name");

        // Label should exist and be properly associated
        var label = Page.Locator("label[for='containerSearch']");
        await Expect(label).ToBeVisibleAsync();
        await Expect(label).ToContainTextAsync("Filter by name");
    }

    #endregion
}
