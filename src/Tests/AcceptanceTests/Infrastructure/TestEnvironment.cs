using System.Diagnostics;

namespace AcceptanceTests.Infrastructure;

/// <summary>
/// Manages the deployment and teardown of the full application stack for acceptance testing.
/// Uses the existing local deploy script to deploy infrastructure and application via Docker.
/// </summary>
public static class TestEnvironment
{
    private static readonly string RepoRoot;
    private static readonly string LocalEnvPath;
    private static bool _isDeployed;

    public static string WebAppUrl { get; } = "http://localhost:4200";
    public static string WebApiUrl { get; } = "http://localhost:5000";

    static TestEnvironment()
    {
        // Find the repository root by searching up for the environments/local directory
        var searchDir = AppContext.BaseDirectory;
        
        while (!string.IsNullOrEmpty(searchDir))
        {
            var envPath = Path.Combine(searchDir, "environments", "local", "deploy.ps1");
            if (File.Exists(envPath))
            {
                RepoRoot = searchDir;
                LocalEnvPath = Path.Combine(searchDir, "environments", "local");
                return;
            }
            searchDir = Path.GetDirectoryName(searchDir);
        }
        
        // Fallback to /workspace if searching fails
        RepoRoot = "/workspace";
        LocalEnvPath = "/workspace/environments/local";
    }

    /// <summary>
    /// Deploys the full application stack using the local deploy script.
    /// </summary>
    public static async Task DeployAsync()
    {
        if (_isDeployed)
        {
            return;
        }

        Console.WriteLine($"Deploying application stack from: {LocalEnvPath}");

        var deployScript = Path.Combine(LocalEnvPath, "deploy.ps1");
        
        if (!File.Exists(deployScript))
        {
            throw new FileNotFoundException($"Deploy script not found at: {deployScript}");
        }

        var processInfo = new ProcessStartInfo
        {
            FileName = "pwsh",
            Arguments = $"-ExecutionPolicy Bypass -File \"{deployScript}\"",
            WorkingDirectory = LocalEnvPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(processInfo);
        if (process == null)
        {
            throw new InvalidOperationException("Failed to start deploy process");
        }

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        
        await process.WaitForExitAsync();

        Console.WriteLine(output);
        if (!string.IsNullOrEmpty(error))
        {
            Console.WriteLine($"Deploy stderr: {error}");
        }

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Deploy script failed with exit code {process.ExitCode}");
        }

        Console.WriteLine("Application stack deployed successfully.");
        
        // Wait for services to be ready
        await WaitForServicesAsync();
        
        _isDeployed = true;
    }

    /// <summary>
    /// Waits for the WebAPI and WebApp to be accessible.
    /// </summary>
    private static async Task WaitForServicesAsync()
    {
        Console.WriteLine("Waiting for services to be ready...");
        
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(5);

        var maxAttempts = 30;
        var delayMs = 2000;

        // Wait for WebAPI
        for (var i = 0; i < maxAttempts; i++)
        {
            try
            {
                var response = await httpClient.GetAsync($"{WebApiUrl}/health");
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("WebAPI is ready.");
                    break;
                }
            }
            catch
            {
                // Service not ready yet
            }

            if (i == maxAttempts - 1)
            {
                throw new TimeoutException("WebAPI did not become ready in time");
            }

            await Task.Delay(delayMs);
        }

        // Wait for WebApp
        for (var i = 0; i < maxAttempts; i++)
        {
            try
            {
                var response = await httpClient.GetAsync(WebAppUrl);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("WebApp is ready.");
                    break;
                }
            }
            catch
            {
                // Service not ready yet
            }

            if (i == maxAttempts - 1)
            {
                throw new TimeoutException("WebApp did not become ready in time");
            }

            await Task.Delay(delayMs);
        }

        Console.WriteLine("All services are ready.");
    }

    /// <summary>
    /// Tears down the application stack (stops containers).
    /// </summary>
    public static async Task TeardownAsync()
    {
        if (!_isDeployed)
        {
            return;
        }

        Console.WriteLine("Tearing down application containers...");

        var processInfo = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = "compose -f docker-compose.app.yml -p inventory_management_app down",
            WorkingDirectory = LocalEnvPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(processInfo);
        if (process != null)
        {
            await process.WaitForExitAsync();
        }

        _isDeployed = false;
        Console.WriteLine("Application containers stopped.");
    }
}
