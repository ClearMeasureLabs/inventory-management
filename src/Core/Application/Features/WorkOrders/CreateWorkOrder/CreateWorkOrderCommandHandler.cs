using Application.DTOs;
using Application.Infrastructure;
using Domain.Entities;

namespace Application.Features.WorkOrders.CreateWorkOrder;

public class CreateWorkOrderCommandHandler : ICreateWorkOrderCommandHandler
{
    private readonly IRepository _repository;
    private readonly ICache _cache;
    private readonly IEventHub _eventHub;

    public CreateWorkOrderCommandHandler(
        IRepository repository,
        ICache cache,
        IEventHub eventHub)
    {
        _repository = repository;
        _cache = cache;
        _eventHub = eventHub;
    }

    public async Task<WorkOrderDto> HandleAsync(CreateWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var workOrder = new WorkOrder
        {
            WorkOrderId = Guid.NewGuid(),
            Title = request.Title
        };

        await _repository.WorkOrders.AddAsync(workOrder, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        await _cache.SetWorkOrderAsync($"{nameof(WorkOrder)}:{workOrder.WorkOrderId}", workOrder);

        await _eventHub.PublishAsync(new WorkOrderCreatedEvent
        {
            WorkOrderId = workOrder.WorkOrderId
        });

        var dto = new WorkOrderDto(workOrder);
        return dto;
    }
}
