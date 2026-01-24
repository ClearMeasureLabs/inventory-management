using Domain.Entities;

namespace Application.Features.ContainerItems;

public class ContainerItemDto
{
    public ContainerItemDto()
    {
    }

    public ContainerItemDto(ContainerItem containerItem)
    {
        ContainerItemId = containerItem.ContainerItemId;
        ContainerId = containerItem.ContainerId;
        ItemId = containerItem.ItemId;
    }

    public int ContainerItemId { get; set; }

    public int ContainerId { get; set; }

    public int ItemId { get; set; }
}
