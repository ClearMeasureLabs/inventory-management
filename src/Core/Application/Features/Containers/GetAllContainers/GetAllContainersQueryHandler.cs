using Application.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Containers.GetAllContainers;

public class GetAllContainersQueryHandler : IGetAllContainersQueryHandler
{
    private readonly IRepository _repository;

    public GetAllContainersQueryHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ContainerDto>> HandleAsync(GetAllContainersQuery query, CancellationToken cancellationToken)
    {
        var containers = await _repository.Containers.ToListAsync(cancellationToken);
        return containers.Select(c => new ContainerDto(c));
    }
}
