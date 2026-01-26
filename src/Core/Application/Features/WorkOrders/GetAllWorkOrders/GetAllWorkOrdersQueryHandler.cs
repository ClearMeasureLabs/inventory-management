using Application.DTOs;
using Application.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.WorkOrders.GetAllWorkOrders;

public class GetAllWorkOrdersQueryHandler : IGetAllWorkOrdersQueryHandler
{
    private readonly IRepository _repository;

    public GetAllWorkOrdersQueryHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<WorkOrderDto>> HandleAsync(GetAllWorkOrdersQuery query, CancellationToken cancellationToken)
    {
        var workOrders = await _repository.WorkOrders.ToListAsync(cancellationToken);
        return workOrders.Select(w => new WorkOrderDto(w));
    }
}
