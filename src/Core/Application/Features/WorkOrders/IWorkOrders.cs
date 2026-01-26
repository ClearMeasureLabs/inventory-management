using Application.DTOs;
using Application.Features.WorkOrders.CreateWorkOrder;
using Application.Features.WorkOrders.DeleteWorkOrder;

namespace Application.Features.WorkOrders;

public interface IWorkOrders
{
    Task<WorkOrderDto> CreateAsync(CreateWorkOrderCommand command, CancellationToken cancellationToken);

    Task DeleteAsync(DeleteWorkOrderCommand command, CancellationToken cancellationToken);

    Task<IEnumerable<WorkOrderDto>> GetAllAsync(CancellationToken cancellationToken);
}
