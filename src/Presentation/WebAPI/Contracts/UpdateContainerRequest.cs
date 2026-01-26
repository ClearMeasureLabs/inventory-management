namespace WebAPI.Contracts;

/// <summary>
/// Request model for updating an existing container.
/// </summary>
public class UpdateContainerRequest
{
    /// <summary>
    /// The name of the container.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The description of the container.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
