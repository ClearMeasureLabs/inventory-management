using Application.Exceptions;
using Application.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Containers.UpdateContainer;

public class UpdateContainerCommandHandler : IUpdateContainerCommandHandler
{
    private readonly IRepository _repository;
    private readonly ICache _cache;
    private readonly IEventHub _eventHub;

    public UpdateContainerCommandHandler(
        IRepository repository,
        ICache cache,
        IEventHub eventHub)
    {
        _repository = repository;
        _cache = cache;
        _eventHub = eventHub;
    }

    private const int NameMaxLength = 200;

    public async Task<ContainerDto> HandleAsync(UpdateContainerCommand request, CancellationToken cancellationToken)
    {
        var container = await _repository.Containers
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

        var errors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors.TryAdd(nameof(request.Name), new List<string>());
            errors[nameof(request.Name)].Add("Name is required");
        }
        else if (request.Name.Length > NameMaxLength)
        {
            errors.TryAdd(nameof(request.Name), new List<string>());
            errors[nameof(request.Name)].Add($"Name cannot exceed {NameMaxLength} characters");
        }
        else
        {
            // Check for duplicate name (excluding current container)
            var duplicateExists = await _repository.Containers
                .AnyAsync(c => c.Name == request.Name && c.ContainerId != request.ContainerId, cancellationToken);

            if (duplicateExists)
            {
                errors.TryAdd(nameof(request.Name), new List<string>());
                errors[nameof(request.Name)].Add("A container with this name already exists");
            }
        }

        if (errors.Count > 0)
        {
            throw new ValidationException
            {
                Errors = errors.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray())
            };
        }

        container.Name = request.Name;
        container.Description = request.Description;

        await _repository.SaveChangesAsync(cancellationToken);
        await _cache.SetAsync($"Container:{container.ContainerId}", container);

        await _eventHub.PublishAsync(new ContainerUpdatedEvent
        {
            ContainerId = container.ContainerId
        }, cancellationToken);

        var dto = new ContainerDto(container);
        return dto;
    }
}
