using Application.Exceptions;
using Application.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.WorkOrders.DeleteWorkOrder;

public class DeleteWorkOrderCommandHandler : IDeleteWorkOrderCommandHandler
{
    private readonly IRepository _repository;
    private readonly ICache _cache;
    private readonly IEventHub _eventHub;

    public DeleteWorkOrderCommandHandler(
        IRepository repository,
        ICache cache,
        IEventHub eventHub)
    {
        _repository = repository;
        _cache = cache;
        _eventHub = eventHub;
    }

    public async Task HandleAsync(DeleteWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _repository.WorkOrders
            .FirstOrDefaultAsync(w => w.WorkOrderId == request.WorkOrderId, cancellationToken);

        if (workOrder is null)
        {
            throw new ValidationException
            {
                Errors = new Dictionary<string, string[]>
                {
                    { "WorkOrderId", new[] { "Work order not found" } }
                }
            };
        }

        _repository.WorkOrders.Remove(workOrder);
        await _repository.SaveChangesAsync(cancellationToken);
        await _cache.RemoveWorkOrderAsync($"WorkOrder:{workOrder.WorkOrderId}", cancellationToken);

        await _eventHub.PublishAsync(new WorkOrderDeletedEvent
        {
            WorkOrderId = workOrder.WorkOrderId
        }, cancellationToken);
    }
}
