using Domain.Entities;

namespace Application.Features.Items;

public class ItemDto
{
    public ItemDto()
    {
    }

    public ItemDto(Item item)
    {
        ItemId = item.ItemId;
        SKU = item.SKU;
        Name = item.Name;
        Description = item.Description;
    }

    public int ItemId { get; set; }

    public string SKU { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}
