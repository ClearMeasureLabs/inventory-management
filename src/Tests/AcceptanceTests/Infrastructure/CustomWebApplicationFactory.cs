using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace AcceptanceTests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory that configures the WebAPI to use
/// infrastructure containers (SQL Server, Redis, RabbitMQ) from TestEnvironment.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly TestEnvironment _testEnvironment;

    public CustomWebApplicationFactory(TestEnvironment testEnvironment)
    {
        _testEnvironment = testEnvironment;
        
        // Set environment variables before WebApplicationFactory starts the host
        // These take precedence over JSON configuration files
        Environment.SetEnvironmentVariable("SqlServer__Host", _testEnvironment.SqlHost);
        Environment.SetEnvironmentVariable("SqlServer__Port", _testEnvironment.SqlPort.ToString());
        Environment.SetEnvironmentVariable("SqlServer__User", "sa");
        Environment.SetEnvironmentVariable("SqlServer__Password", _testEnvironment.SqlPassword);
        Environment.SetEnvironmentVariable("SqlServer__Database", "ivan_acceptance_db");
        Environment.SetEnvironmentVariable("Redis__Host", _testEnvironment.RedisHost);
        Environment.SetEnvironmentVariable("Redis__Port", _testEnvironment.RedisPort.ToString());
        Environment.SetEnvironmentVariable("Redis__User", "default");
        Environment.SetEnvironmentVariable("RabbitMQ__Host", _testEnvironment.RabbitMqHost);
        Environment.SetEnvironmentVariable("RabbitMQ__Port", _testEnvironment.RabbitMqPort.ToString());
        Environment.SetEnvironmentVariable("RabbitMQ__User", _testEnvironment.RabbitMqUser);
        Environment.SetEnvironmentVariable("RabbitMQ__Password", _testEnvironment.RabbitMqPassword);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Override configuration to point to test containers
            var testConfig = new Dictionary<string, string?>
            {
                ["Environment"] = "Test",
                ["Project:Name"] = "Ivan",
                ["Project:Publisher"] = "Clear Measure",
                ["Project:Version"] = "0.0.0",
                ["SqlServer:Host"] = _testEnvironment.SqlHost,
                ["SqlServer:Port"] = _testEnvironment.SqlPort.ToString(),
                ["SqlServer:User"] = "sa",
                ["SqlServer:Password"] = _testEnvironment.SqlPassword,
                ["SqlServer:Database"] = "ivan_acceptance_db",
                ["Redis:Host"] = _testEnvironment.RedisHost,
                ["Redis:Port"] = _testEnvironment.RedisPort.ToString(),
                ["Redis:User"] = "default",
                ["RabbitMQ:Host"] = _testEnvironment.RabbitMqHost,
                ["RabbitMQ:Port"] = _testEnvironment.RabbitMqPort.ToString(),
                ["RabbitMQ:User"] = _testEnvironment.RabbitMqUser,
                ["RabbitMQ:Password"] = _testEnvironment.RabbitMqPassword
            };

            config.AddInMemoryCollection(testConfig);
        });
    }
}
