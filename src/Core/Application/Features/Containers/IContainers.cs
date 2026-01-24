using Application.Features.Containers.CreateContainer;

namespace Application.Features.Containers;

public interface IContainers
{
    Task<ContainerDto> CreateAsync(CreateContainerCommand command, CancellationToken cancellationToken);
}
