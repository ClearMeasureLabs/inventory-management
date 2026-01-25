using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AcceptanceTests.Infrastructure;

/// <summary>
/// Provides access to the WebAPI server for acceptance tests.
/// Starts a real Kestrel server on a dynamic port that can be accessed via HTTP.
/// </summary>
public class ApiServerFixture : IAsyncDisposable
{
    private readonly TestEnvironment _testEnvironment;
    private WebApplicationFactory<Program>? _factory;

    public string ServerAddress { get; private set; } = string.Empty;

    public ApiServerFixture(TestEnvironment testEnvironment)
    {
        _testEnvironment = testEnvironment;
    }

    public async Task StartAsync()
    {
        // Create a WebApplicationFactory that uses Kestrel instead of TestServer
        _factory = new KestrelWebApplicationFactory(_testEnvironment);
        
        // Creating a client triggers the host to start
        _ = _factory.CreateClient();

        // Get the actual server address from Kestrel
        var server = _factory.Services.GetRequiredService<IServer>();
        var addressFeature = server.Features.Get<IServerAddressesFeature>();
        ServerAddress = addressFeature?.Addresses.FirstOrDefault() 
            ?? throw new InvalidOperationException("Could not get server address");
    }

    public async ValueTask DisposeAsync()
    {
        if (_factory != null)
        {
            await _factory.DisposeAsync();
        }
    }

    /// <summary>
    /// Custom WebApplicationFactory that uses Kestrel for real HTTP connections.
    /// </summary>
    private class KestrelWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly TestEnvironment _testEnvironment;
        private IHost? _host;

        public KestrelWebApplicationFactory(TestEnvironment testEnvironment)
        {
            _testEnvironment = testEnvironment;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");

            // Configure to use Kestrel with dynamic port
            builder.UseKestrel();
            builder.UseUrls("http://127.0.0.1:0");

            // Override configuration to point to test containers
            builder.UseSetting("Environment", "Test");
            builder.UseSetting("Project:Name", "Ivan");
            builder.UseSetting("Project:Publisher", "Clear Measure");
            builder.UseSetting("Project:Version", "0.0.0");
            builder.UseSetting("SqlServer:Host", _testEnvironment.SqlHost);
            builder.UseSetting("SqlServer:Port", _testEnvironment.SqlPort.ToString());
            builder.UseSetting("SqlServer:User", "sa");
            builder.UseSetting("SqlServer:Password", _testEnvironment.SqlPassword);
            builder.UseSetting("SqlServer:Database", "ivan_acceptance_db");
            builder.UseSetting("Redis:Host", _testEnvironment.RedisHost);
            builder.UseSetting("Redis:Port", _testEnvironment.RedisPort.ToString());
            builder.UseSetting("Redis:User", "default");
            builder.UseSetting("RabbitMQ:Host", _testEnvironment.RabbitMqHost);
            builder.UseSetting("RabbitMQ:Port", _testEnvironment.RabbitMqPort.ToString());
            builder.UseSetting("RabbitMQ:User", _testEnvironment.RabbitMqUser);
            builder.UseSetting("RabbitMQ:Password", _testEnvironment.RabbitMqPassword);
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            // Create the host but don't use the default TestServer
            var dummyHost = builder.Build();
            
            // Create and start a real Kestrel host
            builder.ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseKestrel();
                webBuilder.UseUrls("http://127.0.0.1:0");
            });

            _host = builder.Build();
            _host.Start();

            return dummyHost;
        }

        public override IServiceProvider Services => _host?.Services ?? base.Services;

        protected override void Dispose(bool disposing)
        {
            _host?.StopAsync().GetAwaiter().GetResult();
            _host?.Dispose();
            base.Dispose(disposing);
        }
    }
}
