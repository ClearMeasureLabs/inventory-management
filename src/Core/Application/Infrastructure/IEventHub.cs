using Application.Features.Containers.CreateContainer;

namespace Application.Infrastructure;

public interface IEventHub : IDisposable
{
    Task PublishAsync(ContainerCreatedEvent @event, CancellationToken cancellationToken = default);
}
