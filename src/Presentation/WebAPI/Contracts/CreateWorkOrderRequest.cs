namespace WebAPI.Contracts;

/// <summary>
/// Request model for creating a new work order.
/// </summary>
public class CreateWorkOrderRequest
{
    /// <summary>
    /// The title of the work order.
    /// </summary>
    public string Title { get; set; } = string.Empty;
}
