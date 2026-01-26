namespace Application.Features.Containers.UpdateContainer;

public interface IUpdateContainerCommandHandler
{
    Task<ContainerDto> HandleAsync(UpdateContainerCommand request, CancellationToken cancellationToken);
}
