using Application.Features.Containers;
using Application.Features.Containers.CreateContainer;
using Application.Features.Containers.GetAllContainers;
using Application.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using RabbitMQ.Client;
using Redis;
using SQLServer;
using IApplication = Application.IApplication;

namespace Bootstrap;

public static class DependencyInjection
{
    public static IConfigurationBuilder GetConfiguration(this IConfigurationBuilder builder, string basePath)
    {
        builder
            .SetBasePath(basePath)
            .AddJsonFile(Path.Combine("config", "local.config.json"), optional: false, reloadOnChange: true)
            .AddJsonFile(Path.Combine("config", "global.config.json"), optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        return builder;
    }

    public static async Task AddAplicationAsync(this IServiceCollection services, IConfiguration configuration)
    {
        // SQL Server / Entity Framework
        var sqlConfig = new SqlServerConfig();

        configuration.GetSection("SqlServer").Bind(sqlConfig);

        var database = configuration.GetValue<string>("SqlServer:Database");

        if (!string.IsNullOrEmpty(database))
        {
            sqlConfig.Database = database;
        }

        services.AddDbContext<InventoryDbContext>(options => options.UseSqlServer(sqlConfig.GetConnectionString()));
        services.AddScoped<IRepository, SQLServerRepository>();

        // Redis Cache
        var redisConfig = new RedisConfig();
        configuration.GetSection("Redis").Bind(redisConfig);

        var redisConnection = await StackExchange.Redis.ConnectionMultiplexer.ConnectAsync(redisConfig.GetConnectionString());
        services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(redisConnection);
        services.AddScoped<ICache, RedisCache>();

        // RabbitMQ Event Hub
        var rabbitConfig = new RabbitMQConfig();
        configuration.GetSection("RabbitMQ").Bind(rabbitConfig);

        var factory = new ConnectionFactory
        {
            HostName = rabbitConfig.Host,
            Port = int.Parse(rabbitConfig.Port),
            UserName = rabbitConfig.User,
            Password = rabbitConfig.Password
        };

        var rabbitConnection = await factory.CreateConnectionAsync();
        var rabbitChannel = await rabbitConnection.CreateChannelAsync();
        services.AddSingleton(rabbitConnection);
        services.AddSingleton(rabbitChannel);
        services.AddSingleton<IEventHub, RabbitMQ.RabbitMQEventHub>();

        // Application services
        services.AddScoped<ICreateContainerCommandHandler, CreateContainerCommandHandler>();
        services.AddScoped<IGetAllContainersQueryHandler, GetAllContainersQueryHandler>();
        services.AddScoped<IContainers, Containers>();
        services.AddScoped<IApplication, Application.Application>();
    }

    public static IServiceCollection AddAllHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var sqlConfig = new SqlServerConfig();
        var redisConfig = new RedisConfig();
        var rabbitConfig = new RabbitMQConfig();
        
        // Load SQL Server config
        configuration.GetSection("SqlServer").Bind(sqlConfig);

        var database = configuration.GetValue<string>("SqlServer:Database");
        
        if (!string.IsNullOrEmpty(database))
        {
            sqlConfig.Database = database;
        }
        
        // Load Redis config
        configuration.GetSection("Redis").Bind(redisConfig);
        
        // Load RabbitMQ config
        configuration.GetSection("RabbitMQ").Bind(rabbitConfig);
        
        var sqlConnectionString = sqlConfig.GetConnectionString();
        var redisConnectionString = redisConfig.GetConnectionString();
        
        services.AddHealthChecks()
            .AddSqlServer(
                connectionString: sqlConnectionString,
                name: "sqlserver",
                tags: new[] { "db", "sql", "sqlserver" })
            .AddRedis(
                redisConnectionString: redisConnectionString,
                name: "redis",
                tags: new[] { "cache", "redis" })
            .AddRabbitMQ(
                sp =>
                {
                    var factory = new ConnectionFactory
                    {
                        HostName = rabbitConfig.Host,
                        Port = int.Parse(rabbitConfig.Port),
                        UserName = rabbitConfig.User,
                        Password = rabbitConfig.Password
                    };
                    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
                },
                name: "rabbitmq",
                tags: new[] { "messaging", "rabbitmq" });
        
        return services;
    }

    public static void AddOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var projectName = configuration.GetValue<string>("Project:Name");
        var environment = configuration.GetValue<string>("Environment");

        // TODO: add application insights for non-local environments

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService($"{projectName} Web API"))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddConsoleExporter();
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddConsoleExporter();
            })
            .WithLogging(logging =>
            {
                logging.AddConsoleExporter();
            });
    }
}


