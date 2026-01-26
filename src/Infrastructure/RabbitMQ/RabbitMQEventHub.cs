using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Features.Containers.CreateContainer;
using Application.Features.Containers.DeleteContainer;
using Application.Features.WorkOrders.CreateWorkOrder;
using Application.Features.WorkOrders.DeleteWorkOrder;
using Application.Infrastructure;
using RabbitMQ.Client;

namespace RabbitMQ;

public class RabbitMQEventHub : IEventHub
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly HashSet<string> _declaredExchanges = [];
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private bool _disposed;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public RabbitMQEventHub(IConnection connection, IChannel channel)
    {
        _connection = connection;
        _channel = channel;
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
        _semaphore.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task PublishAsync(ContainerCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        var exchangeName = nameof(ContainerCreatedEvent);
        await EnsureExchangeExistsAsync(exchangeName, cancellationToken);

        var message = JsonSerializer.Serialize(@event, _jsonOptions);
        var body = Encoding.UTF8.GetBytes(message);

        await _channel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: string.Empty,
            body: body,
            cancellationToken: cancellationToken);
    }

    public async Task PublishAsync(ContainerDeletedEvent @event, CancellationToken cancellationToken = default)
    {
        var exchangeName = nameof(ContainerDeletedEvent);
        await EnsureExchangeExistsAsync(exchangeName, cancellationToken);

        var message = JsonSerializer.Serialize(@event, _jsonOptions);
        var body = Encoding.UTF8.GetBytes(message);

        await _channel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: string.Empty,
            body: body,
            cancellationToken: cancellationToken);
    }

    public async Task PublishAsync(WorkOrderCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        var exchangeName = nameof(WorkOrderCreatedEvent);
        await EnsureExchangeExistsAsync(exchangeName, cancellationToken);

        var message = JsonSerializer.Serialize(@event, _jsonOptions);
        var body = Encoding.UTF8.GetBytes(message);

        await _channel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: string.Empty,
            body: body,
            cancellationToken: cancellationToken);
    }

    public async Task PublishAsync(WorkOrderDeletedEvent @event, CancellationToken cancellationToken = default)
    {
        var exchangeName = nameof(WorkOrderDeletedEvent);
        await EnsureExchangeExistsAsync(exchangeName, cancellationToken);

        var message = JsonSerializer.Serialize(@event, _jsonOptions);
        var body = Encoding.UTF8.GetBytes(message);

        await _channel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: string.Empty,
            body: body,
            cancellationToken: cancellationToken);
    }

    private async Task EnsureExchangeExistsAsync(string exchangeName, CancellationToken cancellationToken)
    {
        if (_declaredExchanges.Contains(exchangeName))
        {
            return;
        }

        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            if (_declaredExchanges.Contains(exchangeName))
            {
                return;
            }

            await _channel.ExchangeDeclareAsync(
                exchange: exchangeName,
                type: ExchangeType.Fanout,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken);

            _declaredExchanges.Add(exchangeName);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
