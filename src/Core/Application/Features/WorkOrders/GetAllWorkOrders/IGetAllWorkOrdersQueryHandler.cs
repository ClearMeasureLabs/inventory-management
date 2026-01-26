using Application.DTOs;

namespace Application.Features.WorkOrders.GetAllWorkOrders;

public interface IGetAllWorkOrdersQueryHandler
{
    Task<IEnumerable<WorkOrderDto>> HandleAsync(GetAllWorkOrdersQuery query, CancellationToken cancellationToken);
}
