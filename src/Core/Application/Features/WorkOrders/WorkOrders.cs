using Application.DTOs;
using Application.Features.WorkOrders.CreateWorkOrder;
using Application.Features.WorkOrders.DeleteWorkOrder;
using Application.Features.WorkOrders.GetAllWorkOrders;

namespace Application.Features.WorkOrders;

public class WorkOrders : IWorkOrders
{
    private readonly ICreateWorkOrderCommandHandler _createWorkOrderCommandHandler;
    private readonly IDeleteWorkOrderCommandHandler _deleteWorkOrderCommandHandler;
    private readonly IGetAllWorkOrdersQueryHandler _getAllWorkOrdersQueryHandler;

    public WorkOrders(
        ICreateWorkOrderCommandHandler createWorkOrderCommandHandler,
        IDeleteWorkOrderCommandHandler deleteWorkOrderCommandHandler,
        IGetAllWorkOrdersQueryHandler getAllWorkOrdersQueryHandler)
    {
        _createWorkOrderCommandHandler = createWorkOrderCommandHandler;
        _deleteWorkOrderCommandHandler = deleteWorkOrderCommandHandler;
        _getAllWorkOrdersQueryHandler = getAllWorkOrdersQueryHandler;
    }

    public Task<WorkOrderDto> CreateAsync(CreateWorkOrderCommand command, CancellationToken cancellationToken)
    {
        return _createWorkOrderCommandHandler.HandleAsync(command, cancellationToken);
    }

    public Task DeleteAsync(DeleteWorkOrderCommand command, CancellationToken cancellationToken)
    {
        return _deleteWorkOrderCommandHandler.HandleAsync(command, cancellationToken);
    }

    public Task<IEnumerable<WorkOrderDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return _getAllWorkOrdersQueryHandler.HandleAsync(new GetAllWorkOrdersQuery(), cancellationToken);
    }
}
