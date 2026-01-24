using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace AcceptanceTests.Infrastructure;

public class WebAppFixture : WebApplicationFactory<Program>
{
    private readonly TestEnvironment _testEnvironment;

    public WebAppFixture(TestEnvironment testEnvironment)
    {
        _testEnvironment = testEnvironment;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Clear existing configuration sources
            config.Sources.Clear();

            // Add test configuration with container connection details
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
                ["SqlServer:Database"] = "ivan_test_db",
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

        builder.UseEnvironment("Development");
    }
}
