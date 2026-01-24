using Application.DTOs;
using Domain.Entities;
using System.Text.Json.Serialization;

namespace Application.Features.Containers;

public class ContainerDto
{
    public ContainerDto()
    {
    }

    public ContainerDto(Container container)
    {
        ContainerId = container.ContainerId;
        Name = container.Name;
        Description = container.Description;

        InventoryItems = container.InventoryItems is null
            ? new List<ContainerItemDto>()
            : new List<ContainerItemDto>(container.InventoryItems.Select(ci => new ContainerItemDto(ci)));
    }

    public int ContainerId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<ContainerItemDto> InventoryItems { get; set; } = new List<ContainerItemDto>();
}
