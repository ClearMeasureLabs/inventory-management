namespace Application;

public class Application : IApplication
{
    public Application(Features.Containers.IContainers containers)
    {
        Containers = containers;
    }

    public Features.Containers.IContainers Containers { get; }
}
