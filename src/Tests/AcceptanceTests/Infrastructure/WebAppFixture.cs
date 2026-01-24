using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace AcceptanceTests.Infrastructure;

public class WebAppFixture : WebApplicationFactory<Program>
{
    private readonly TestEnvironment _testEnvironment;

    public WebAppFixture(TestEnvironment testEnvironment)
    {
        _testEnvironment = testEnvironment;

        // Set environment variables BEFORE the application starts
        // These override values from config files since AddEnvironmentVariables() is called in GetConfiguration
        Environment.SetEnvironmentVariable("Environment", "Test");
        Environment.SetEnvironmentVariable("Project__Name", "Ivan");
        Environment.SetEnvironmentVariable("Project__Publisher", "Clear Measure");
        Environment.SetEnvironmentVariable("Project__Version", "0.0.0");
        Environment.SetEnvironmentVariable("SqlServer__Host", _testEnvironment.SqlHost);
        Environment.SetEnvironmentVariable("SqlServer__Port", _testEnvironment.SqlPort.ToString());
        Environment.SetEnvironmentVariable("SqlServer__User", "sa");
        Environment.SetEnvironmentVariable("SqlServer__Password", _testEnvironment.SqlPassword);
        Environment.SetEnvironmentVariable("SqlServer__Database", "ivan_test_db");
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
        builder.UseEnvironment("Development");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Clean up environment variables
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

        base.Dispose(disposing);
    }
}
