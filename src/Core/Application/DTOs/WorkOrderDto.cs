using Domain.Entities;

namespace Application.DTOs;

public class WorkOrderDto
{
    public WorkOrderDto()
    {
    }

    public WorkOrderDto(WorkOrder workOrder)
    {
        WorkOrderId = workOrder.WorkOrderId;
        Title = workOrder.Title;
    }

    public Guid WorkOrderId { get; set; }

    public string Title { get; set; } = string.Empty;
}
