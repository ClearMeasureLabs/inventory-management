using Application.DTOs;

namespace Application.Features.WorkOrders.CreateWorkOrder;

public interface ICreateWorkOrderCommandHandler
{
    Task<WorkOrderDto> HandleAsync(CreateWorkOrderCommand request, CancellationToken cancellationToken);
}
