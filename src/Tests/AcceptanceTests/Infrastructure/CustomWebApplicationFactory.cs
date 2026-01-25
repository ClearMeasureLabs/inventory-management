using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace AcceptanceTests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory that configures the WebApp to use
/// infrastructure containers (SQL Server, Redis, RabbitMQ) from TestEnvironment.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly TestEnvironment _testEnvironment;

    public CustomWebApplicationFactory(TestEnvironment testEnvironment)
    {
        _testEnvironment = testEnvironment;
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
