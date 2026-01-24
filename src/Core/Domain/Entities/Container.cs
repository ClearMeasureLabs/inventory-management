using System.Text.Json.Serialization;

namespace Domain.Entities;

public class Container
{
    public int ContainerId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<ContainerItem> InventoryItems { get; set; } = [];
}
