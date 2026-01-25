namespace WebAPI.Contracts;

/// <summary>
/// Request model for creating a new container.
/// </summary>
public class CreateContainerRequest
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
