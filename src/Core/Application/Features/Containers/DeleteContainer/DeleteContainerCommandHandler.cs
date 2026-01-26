using Application.Exceptions;
using Application.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Containers.DeleteContainer;

public class DeleteContainerCommandHandler : IDeleteContainerCommandHandler
{
    private readonly IRepository _repository;
    private readonly ICache _cache;
    private readonly IEventHub _eventHub;

    public DeleteContainerCommandHandler(
        IRepository repository,
        ICache cache,
        IEventHub eventHub)
    {
        _repository = repository;
        _cache = cache;
        _eventHub = eventHub;
    }

    public async Task HandleAsync(DeleteContainerCommand request, CancellationToken cancellationToken)
    {
        var container = await _repository.Containers
            .Include(c => c.InventoryItems)
            .FirstOrDefaultAsync(c => c.ContainerId == request.ContainerId, cancellationToken);

        if (container is null)
        {
            throw new ValidationException
            {
                Errors = new Dictionary<string, string[]>
                {
                    { "ContainerId", new[] { "Container not found" } }
                }
            };
        }

        if (container.InventoryItems.Count > 0)
        {
            throw new ValidationException
            {
                Errors = new Dictionary<string, string[]>
                {
                    { "ContainerId", new[] { "Cannot delete a container that has items" } }
                }
            };
        }

        _repository.Containers.Remove(container);
        await _repository.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync($"Container:{container.ContainerId}", cancellationToken);

        await _eventHub.PublishAsync(new ContainerDeletedEvent
        {
            ContainerId = container.ContainerId
        }, cancellationToken);
    }
}
