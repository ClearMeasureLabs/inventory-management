using Application.Features.Containers;

namespace WebAPI.Contracts;

/// <summary>
/// Response model for container data.
/// </summary>
public class ContainerResponse
{
    public ContainerResponse()
    {
    }

    public ContainerResponse(ContainerDto dto)
    {
        ContainerId = dto.ContainerId;
        Name = dto.Name;
        Description = dto.Description;
    }

    /// <summary>
    /// The unique identifier of the container.
    /// </summary>
    public int ContainerId { get; set; }

    /// <summary>
    /// The name of the container.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The description of the container.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Maps a ContainerDto to a ContainerResponse.
    /// </summary>
    public static ContainerResponse FromDto(ContainerDto dto) => new(dto);
}
