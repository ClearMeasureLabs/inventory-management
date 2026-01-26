using AcceptanceTests.Infrastructure;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace AcceptanceTests;

/// <summary>
/// Acceptance tests to verify the deployed application stack is accessible and functional.
/// </summary>
[TestFixture]
public class HealthCheckTests : PageTest
{
    [Test]
    public async Task WebAPI_HealthEndpoint_ReturnsSuccess()
    {
        // Arrange
        var healthUrl = $"{TestEnvironment.WebApiUrl}/health";

        // Act
        var response = await Page.APIRequest.GetAsync(healthUrl);

        // Assert
        response.Ok.ShouldBeTrue($"Health endpoint returned status {response.Status}");
    }

    [Test]
    public async Task WebApp_HomePage_Loads()
    {
        // Arrange & Act
        await Page.GotoAsync(TestEnvironment.WebAppUrl);

        // Assert
        var title = await Page.TitleAsync();
        title.ShouldNotBeNullOrEmpty("Page should have a title");
        
        // Verify the page has loaded by checking for expected content
        await Expect(Page.Locator("body")).ToBeVisibleAsync();
    }

    [Test]
    public async Task WebApp_NavigationBar_IsVisible()
    {
        // Arrange
        await Page.GotoAsync(TestEnvironment.WebAppUrl);

        // Act & Assert
        var nav = Page.Locator("nav");
        await Expect(nav).ToBeVisibleAsync();
    }
}
