using Application.Features.Containers.CreateContainer;
using Application.Features.Containers.DeleteContainer;
using Application.Features.Containers.UpdateContainer;

namespace Application.Infrastructure;

public interface IEventHub : IDisposable
{
    Task PublishAsync(ContainerCreatedEvent @event, CancellationToken cancellationToken = default);

    Task PublishAsync(ContainerDeletedEvent @event, CancellationToken cancellationToken = default);

    Task PublishAsync(ContainerUpdatedEvent @event, CancellationToken cancellationToken = default);
}
