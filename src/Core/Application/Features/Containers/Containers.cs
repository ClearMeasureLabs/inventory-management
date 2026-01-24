using Application.Features.Containers.CreateContainer;

namespace Application.Features.Containers;

public class Containers : IContainers
{
    private readonly ICreateContainerCommandHandler _createContainerCommandHandler;

    public Containers(ICreateContainerCommandHandler createContainerCommandHandler)
    {
        _createContainerCommandHandler = createContainerCommandHandler;
    }

    public Task<ContainerDto> CreateAsync(CreateContainerCommand command, CancellationToken cancellationToken)
    {
        return _createContainerCommandHandler.HandleAsync(command, cancellationToken);
    }
}
