namespace Application.Features.Containers.GetContainerById;

public interface IGetContainerByIdQueryHandler
{
    Task<ContainerDto?> HandleAsync(GetContainerByIdQuery query, CancellationToken cancellationToken);
}
