using Bootstrap;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SQLServer;
using WebApp.Components;

namespace AcceptanceTests.Infrastructure;

/// <summary>
/// Starts a real Kestrel server for Playwright browser tests.
/// This creates an actual HTTP server that Playwright can connect to.
/// </summary>
public class PlaywrightServerFixture : IAsyncDisposable
{
    private WebApplication? _app;
    private readonly TestEnvironment _testEnvironment;

    public string ServerAddress { get; private set; } = string.Empty;

    public PlaywrightServerFixture(TestEnvironment testEnvironment)
    {
        _testEnvironment = testEnvironment;
    }

    public async Task StartAsync()
    {
        // Find a free port
        var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        ServerAddress = $"http://127.0.0.1:{port}";

        // Set environment variables
        SetEnvironmentVariables();

        // Get the WebApp's output directory for proper content root
        var webAppAssembly = typeof(Program).Assembly;
        var webAppPath = Path.GetDirectoryName(webAppAssembly.Location)
            ?? throw new DirectoryNotFoundException("WebApp assembly location not found");

        // Create the web application builder
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Development",
            ContentRootPath = webAppPath,
            WebRootPath = Path.Combine(webAppPath, "wwwroot")
        });

        // Configure to use our port
        builder.WebHost.UseUrls(ServerAddress);

        // Load configuration (same as Program.cs)
        builder.Configuration.GetConfiguration(webAppPath)
            .AddJsonFile(Path.Combine(webAppPath, "appsettings.json"), optional: true);

        // Add services (matching Program.cs)
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();

        builder.Services.AddControllers();

        await builder.Services.AddAplicationAsync(builder.Configuration);
        builder.Services.AddAllHealthChecks(builder.Configuration);

        _app = builder.Build();

        // Run migrations
        using (var scope = _app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            await dbContext.Database.MigrateAsync();
        }

        // Configure middleware (matching Program.cs for Development)
        _app.UseWebAssemblyDebugging();
        _app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        _app.UseStaticFiles();
        _app.UseAntiforgery();
        _app.MapControllers();
        _app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(WebApp.Client._Imports).Assembly);

        await _app.StartAsync();
    }

    private void SetEnvironmentVariables()
    {
        Environment.SetEnvironmentVariable("Environment", "Test");
        Environment.SetEnvironmentVariable("Project__Name", "Ivan");
        Environment.SetEnvironmentVariable("Project__Publisher", "Clear Measure");
        Environment.SetEnvironmentVariable("Project__Version", "0.0.0");
        Environment.SetEnvironmentVariable("SqlServer__Host", _testEnvironment.SqlHost);
        Environment.SetEnvironmentVariable("SqlServer__Port", _testEnvironment.SqlPort.ToString());
        Environment.SetEnvironmentVariable("SqlServer__User", "sa");
        Environment.SetEnvironmentVariable("SqlServer__Password", _testEnvironment.SqlPassword);
        Environment.SetEnvironmentVariable("SqlServer__Database", "ivan_playwright_db");
        Environment.SetEnvironmentVariable("Redis__Host", _testEnvironment.RedisHost);
        Environment.SetEnvironmentVariable("Redis__Port", _testEnvironment.RedisPort.ToString());
        Environment.SetEnvironmentVariable("Redis__User", "default");
        Environment.SetEnvironmentVariable("RabbitMQ__Host", _testEnvironment.RabbitMqHost);
        Environment.SetEnvironmentVariable("RabbitMQ__Port", _testEnvironment.RabbitMqPort.ToString());
        Environment.SetEnvironmentVariable("RabbitMQ__User", _testEnvironment.RabbitMqUser);
        Environment.SetEnvironmentVariable("RabbitMQ__Password", _testEnvironment.RabbitMqPassword);
    }

    private void ClearEnvironmentVariables()
    {
        Environment.SetEnvironmentVariable("Environment", null);
        Environment.SetEnvironmentVariable("Project__Name", null);
        Environment.SetEnvironmentVariable("Project__Publisher", null);
        Environment.SetEnvironmentVariable("Project__Version", null);
        Environment.SetEnvironmentVariable("SqlServer__Host", null);
        Environment.SetEnvironmentVariable("SqlServer__Port", null);
        Environment.SetEnvironmentVariable("SqlServer__User", null);
        Environment.SetEnvironmentVariable("SqlServer__Password", null);
        Environment.SetEnvironmentVariable("SqlServer__Database", null);
        Environment.SetEnvironmentVariable("Redis__Host", null);
        Environment.SetEnvironmentVariable("Redis__Port", null);
        Environment.SetEnvironmentVariable("Redis__User", null);
        Environment.SetEnvironmentVariable("RabbitMQ__Host", null);
        Environment.SetEnvironmentVariable("RabbitMQ__Port", null);
        Environment.SetEnvironmentVariable("RabbitMQ__User", null);
        Environment.SetEnvironmentVariable("RabbitMQ__Password", null);
    }

    public async ValueTask DisposeAsync()
    {
        if (_app != null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
        ClearEnvironmentVariables();
    }
}
