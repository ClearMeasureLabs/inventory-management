using Application.Features.Containers.CreateContainer;
using Application.Features.Containers.GetAllContainers;

namespace Application.Features.Containers;

public interface IContainers
{
    Task<ContainerDto> CreateAsync(CreateContainerCommand command, CancellationToken cancellationToken);

    Task<IEnumerable<ContainerDto>> GetAllAsync(CancellationToken cancellationToken);
}
