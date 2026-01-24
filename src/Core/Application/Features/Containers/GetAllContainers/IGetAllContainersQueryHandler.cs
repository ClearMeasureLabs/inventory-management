namespace Application.Features.Containers.GetAllContainers;

public interface IGetAllContainersQueryHandler
{
    Task<IEnumerable<ContainerDto>> HandleAsync(GetAllContainersQuery query, CancellationToken cancellationToken);
}
