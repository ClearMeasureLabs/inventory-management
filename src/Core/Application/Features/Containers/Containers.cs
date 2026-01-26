using Application.Features.Containers.CreateContainer;
using Application.Features.Containers.DeleteContainer;
using Application.Features.Containers.GetAllContainers;

namespace Application.Features.Containers;

public class Containers : IContainers
{
    private readonly ICreateContainerCommandHandler _createContainerCommandHandler;
    private readonly IDeleteContainerCommandHandler _deleteContainerCommandHandler;
    private readonly IGetAllContainersQueryHandler _getAllContainersQueryHandler;

    public Containers(
        ICreateContainerCommandHandler createContainerCommandHandler,
        IDeleteContainerCommandHandler deleteContainerCommandHandler,
        IGetAllContainersQueryHandler getAllContainersQueryHandler)
    {
        _createContainerCommandHandler = createContainerCommandHandler;
        _deleteContainerCommandHandler = deleteContainerCommandHandler;
        _getAllContainersQueryHandler = getAllContainersQueryHandler;
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
}
