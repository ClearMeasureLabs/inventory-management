using Bootstrap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SQLServer;

namespace IntegrationTests.Infrastructure;

public class ServiceProviderBuilder
{
    private readonly TestEnvironment _testEnvironment;

    public ServiceProviderBuilder(TestEnvironment testEnvironment)
    {
        _testEnvironment = testEnvironment;
    }

    public async Task<IServiceProvider> BuildAsync()
    {
        var configuration = BuildConfiguration();
        var services = new ServiceCollection();

        await services.AddAplicationAsync(configuration);

        var serviceProvider = services.BuildServiceProvider();

        // Ensure database is created
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        return serviceProvider;
    }

    private IConfiguration BuildConfiguration()
    {
        var configValues = new Dictionary<string, string?>
        {
            ["Environment"] = "Test",
            ["Project:Name"] = "Ivan",
            ["Project:Publisher"] = "Clear Measure",
            ["Project:Version"] = "0.0.0",
            ["SqlServer:Host"] = _testEnvironment.SqlHost,
            ["SqlServer:Port"] = _testEnvironment.SqlPort.ToString(),
            ["SqlServer:User"] = "sa",
            ["SqlServer:Password"] = _testEnvironment.SqlPassword,
            ["SqlServer:Database"] = "ivan_integration_test_db",
            ["Redis:Host"] = _testEnvironment.RedisHost,
            ["Redis:Port"] = _testEnvironment.RedisPort.ToString(),
            ["Redis:User"] = "default",
            ["RabbitMQ:Host"] = _testEnvironment.RabbitMqHost,
            ["RabbitMQ:Port"] = _testEnvironment.RabbitMqPort.ToString(),
            ["RabbitMQ:User"] = _testEnvironment.RabbitMqUser,
            ["RabbitMQ:Password"] = _testEnvironment.RabbitMqPassword
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();
    }
}
