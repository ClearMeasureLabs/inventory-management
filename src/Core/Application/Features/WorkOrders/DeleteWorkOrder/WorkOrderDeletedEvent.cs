namespace Application.Features.WorkOrders.DeleteWorkOrder;

public class WorkOrderDeletedEvent
{
    public Guid WorkOrderId { get; set; }
}
