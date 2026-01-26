using Application.Features.Containers.CreateContainer;
using Application.Features.Containers.DeleteContainer;
using Application.Features.Containers.GetAllContainers;
using Application.Features.Containers.UpdateContainer;

namespace Application.Features.Containers;

public class Containers : IContainers
{
    private readonly ICreateContainerCommandHandler _createContainerCommandHandler;
    private readonly IDeleteContainerCommandHandler _deleteContainerCommandHandler;
    private readonly IGetAllContainersQueryHandler _getAllContainersQueryHandler;
    private readonly IUpdateContainerCommandHandler _updateContainerCommandHandler;

    public Containers(
        ICreateContainerCommandHandler createContainerCommandHandler,
        IDeleteContainerCommandHandler deleteContainerCommandHandler,
        IGetAllContainersQueryHandler getAllContainersQueryHandler,
        IUpdateContainerCommandHandler updateContainerCommandHandler)
    {
        _createContainerCommandHandler = createContainerCommandHandler;
        _deleteContainerCommandHandler = deleteContainerCommandHandler;
        _getAllContainersQueryHandler = getAllContainersQueryHandler;
        _updateContainerCommandHandler = updateContainerCommandHandler;
    }

    public Task<ContainerDto> CreateAsync(CreateContainerCommand command, CancellationToken cancellationToken)
    {
        return _createContainerCommandHandler.HandleAsync(command, cancellationToken);
    }

    public Task DeleteAsync(DeleteContainerCommand command, CancellationToken cancellationToken)
    {
        return _deleteContainerCommandHandler.HandleAsync(command, cancellationToken);
    }

    public Task<IEnumerable<ContainerDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return _getAllContainersQueryHandler.HandleAsync(new GetAllContainersQuery(), cancellationToken);
    }

    public Task<ContainerDto> UpdateAsync(UpdateContainerCommand command, CancellationToken cancellationToken)
    {
        return _updateContainerCommandHandler.HandleAsync(command, cancellationToken);
    }
}
