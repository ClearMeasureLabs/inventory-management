namespace Application.Features.WorkOrders.DeleteWorkOrder;

public interface IDeleteWorkOrderCommandHandler
{
    Task HandleAsync(DeleteWorkOrderCommand request, CancellationToken cancellationToken);
}
