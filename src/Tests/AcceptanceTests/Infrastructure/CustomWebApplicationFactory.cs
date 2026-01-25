using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

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

        // Set environment variables BEFORE the host is created.
        // These are picked up by AddEnvironmentVariables() in DependencyInjection.GetConfiguration()
        // which runs during Program.cs execution, before ConfigureAppConfiguration would apply.
        Environment.SetEnvironmentVariable("Environment", "Test");
        Environment.SetEnvironmentVariable("Project__Name", "Ivan");
        Environment.SetEnvironmentVariable("Project__Publisher", "Clear Measure");
        Environment.SetEnvironmentVariable("Project__Version", "0.0.0");
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
    }
}
