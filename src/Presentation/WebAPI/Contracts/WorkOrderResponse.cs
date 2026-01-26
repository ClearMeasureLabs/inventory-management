using Application.DTOs;

namespace WebAPI.Contracts;

/// <summary>
/// Response model for work order data.
/// </summary>
public class WorkOrderResponse
{
    public WorkOrderResponse()
    {
    }

    public WorkOrderResponse(WorkOrderDto dto)
    {
        WorkOrderId = dto.WorkOrderId;
        Title = dto.Title;
    }

    /// <summary>
    /// The unique identifier of the work order.
    /// </summary>
    public Guid WorkOrderId { get; set; }

    /// <summary>
    /// The title of the work order.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Maps a WorkOrderDto to a WorkOrderResponse.
    /// </summary>
    public static WorkOrderResponse FromDto(WorkOrderDto dto) => new(dto);
}
