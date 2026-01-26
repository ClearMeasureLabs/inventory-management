using Application.Features.Containers.CreateContainer;
using Application.Features.Containers.DeleteContainer;
using Application.Features.WorkOrders.CreateWorkOrder;
using Application.Features.WorkOrders.DeleteWorkOrder;

namespace Application.Infrastructure;

public interface IEventHub : IDisposable
{
    Task PublishAsync(ContainerCreatedEvent @event, CancellationToken cancellationToken = default);

    Task PublishAsync(ContainerDeletedEvent @event, CancellationToken cancellationToken = default);

    Task PublishAsync(WorkOrderCreatedEvent @event, CancellationToken cancellationToken = default);

    Task PublishAsync(WorkOrderDeletedEvent @event, CancellationToken cancellationToken = default);
}
