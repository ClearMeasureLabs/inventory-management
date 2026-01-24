namespace Domain.Entities;

public class Item
{
    public int ItemId { get; set; }

    public string SKU { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}
