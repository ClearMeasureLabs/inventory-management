namespace Domain.Entities;

public class WorkOrder
{
    public Guid WorkOrderId { get; set; }

    public string Title { get; set; } = string.Empty;
}
