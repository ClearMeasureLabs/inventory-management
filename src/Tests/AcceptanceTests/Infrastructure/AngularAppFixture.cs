using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.Text.RegularExpressions;

namespace AcceptanceTests.Infrastructure;

/// <summary>
/// Serves the Angular application static files for Playwright browser tests.
/// Configures the Angular app to point to the API server.
/// </summary>
public class AngularAppFixture : IAsyncDisposable
{
    private IHost? _host;
    private readonly string _angularDistPath;

    public string ServerAddress { get; private set; } = string.Empty;

    public AngularAppFixture()
    {
        // Calculate the path to the Angular dist folder
        var currentDir = Directory.GetCurrentDirectory();
        
        // Try to find the Angular dist folder relative to the test execution directory
        var possiblePaths = new[]
        {
            Path.Combine(currentDir, "..", "..", "..", "..", "..", "Presentation", "webapp", "dist", "webapp", "browser"),
            Path.Combine(currentDir, "..", "..", "..", "..", "Presentation", "webapp", "dist", "webapp", "browser"),
            Path.Combine(currentDir, "Presentation", "webapp", "dist", "webapp", "browser"),
            // Fallback for running from solution root
            Path.GetFullPath(Path.Combine(currentDir, "src", "Presentation", "webapp", "dist", "webapp", "browser"))
        };

        _angularDistPath = possiblePaths.FirstOrDefault(Directory.Exists) 
            ?? throw new DirectoryNotFoundException(
                $"Could not find Angular dist folder. Looked in: {string.Join(", ", possiblePaths)}. " +
                $"Current directory: {currentDir}. Make sure to run 'npm run build' in the Angular project first.");
    }

    public async Task StartAsync(string apiServerAddress)
    {
        // Update the environment configuration in the Angular app
        await UpdateAngularEnvironmentAsync(apiServerAddress);

        var builder = WebApplication.CreateBuilder();
        
        builder.WebHost.UseKestrel();
        builder.WebHost.UseUrls("http://127.0.0.1:0");

        var app = builder.Build();

        // Serve static files from Angular dist
        app.UseDefaultFiles(new DefaultFilesOptions
        {
            FileProvider = new PhysicalFileProvider(_angularDistPath)
        });
        
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(_angularDistPath)
        });

        // Fallback to index.html for Angular routing
        app.MapFallbackToFile("index.html", new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(_angularDistPath)
        });

        _host = app;
        await _host.StartAsync();

        // Get the actual server address
        var addresses = app.Urls;
        ServerAddress = addresses.FirstOrDefault() 
            ?? throw new InvalidOperationException("Could not get Angular server address");
    }

    private async Task UpdateAngularEnvironmentAsync(string apiServerAddress)
    {
        // Find and update the main JavaScript bundle to inject the API URL
        var jsFiles = Directory.GetFiles(_angularDistPath, "main-*.js");
        
        foreach (var jsFile in jsFiles)
        {
            var content = await File.ReadAllTextAsync(jsFile);
            
            // Replace the apiUrl in the bundled JavaScript
            // The environment.ts gets compiled into the bundle
            // Look for patterns like: apiUrl:"http://localhost:5000" or apiUrl: "http://localhost:5000"
            var patterns = new[]
            {
                @"apiUrl:\s*""[^""]*""",
                @"apiUrl:\s*'[^']*'"
            };

            foreach (var pattern in patterns)
            {
                content = Regex.Replace(content, pattern, $"apiUrl:\"{apiServerAddress}\"");
            }

            await File.WriteAllTextAsync(jsFile, content);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
    }
}
