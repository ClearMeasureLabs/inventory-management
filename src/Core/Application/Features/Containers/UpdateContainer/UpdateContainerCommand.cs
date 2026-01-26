namespace Application.Features.Containers.UpdateContainer;

public class UpdateContainerCommand
{
    public int ContainerId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}
