using Application.Features.Containers;

namespace Application;

public interface IApplication
{
    IContainers Containers { get; }
}
