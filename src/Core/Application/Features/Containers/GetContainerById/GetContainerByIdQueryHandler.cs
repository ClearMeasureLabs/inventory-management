using Application.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Containers.GetContainerById;

public class GetContainerByIdQueryHandler : IGetContainerByIdQueryHandler
{
    private readonly IRepository _repository;

    public GetContainerByIdQueryHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<ContainerDto?> HandleAsync(GetContainerByIdQuery query, CancellationToken cancellationToken)
    {
        var container = await _repository.Containers
            .FirstOrDefaultAsync(c => c.ContainerId == query.ContainerId, cancellationToken);

        return container is null ? null : new ContainerDto(container);
    }
}
