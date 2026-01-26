namespace Application.Features.Containers.DeleteContainer;

public interface IDeleteContainerCommandHandler
{
    Task HandleAsync(DeleteContainerCommand request, CancellationToken cancellationToken);
}
