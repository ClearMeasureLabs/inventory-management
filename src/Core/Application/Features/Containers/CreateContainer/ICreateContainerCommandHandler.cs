namespace Application.Features.Containers.CreateContainer;

public interface ICreateContainerCommandHandler
{
    Task<ContainerDto> HandleAsync(CreateContainerCommand request, CancellationToken cancellationToken);
}