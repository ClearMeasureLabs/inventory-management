using Application.Features.Containers.CreateContainer;
using Application.Features.Containers.DeleteContainer;
using Application.Features.Containers.GetAllContainers;

namespace Application.Features.Containers;

public interface IContainers
{
    Task<ContainerDto> CreateAsync(CreateContainerCommand command, CancellationToken cancellationToken);

    Task DeleteAsync(DeleteContainerCommand command, CancellationToken cancellationToken);

    Task<IEnumerable<ContainerDto>> GetAllAsync(CancellationToken cancellationToken);
}
