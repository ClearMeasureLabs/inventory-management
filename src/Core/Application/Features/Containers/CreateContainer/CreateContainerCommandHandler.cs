using Application.Exceptions;
using Application.Infrastructure;
using Domain.Entities;

namespace Application.Features.Containers.CreateContainer;

public class CreateContainerCommandHandler : ICreateContainerCommandHandler
{
    private readonly IRepository _repository;
    private readonly ICache _cache;
    private readonly IEventHub _eventHub;

    public CreateContainerCommandHandler(
        IRepository repository,
        ICache cache,
        IEventHub eventHub)
    {
        _repository = repository;
        _cache = cache;
        _eventHub = eventHub;
    }

    public async Task<ContainerDto> HandleAsync(CreateContainerCommand request, CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors.TryAdd(nameof(request.Name), new List<string>());
            errors[nameof(request.Name)].Add("Name is required");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            errors.TryAdd(nameof(request.Description), new List<string>());
            errors[nameof(request.Description)].Add("Description is required");
        }

        if (errors.Count > 0)
        {
            throw new ValidationException
            {
                Errors = errors.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray())
            };
        }

        var container = new Container
        {
            Name = request.Name,
            Description = request.Description
        };

        await _repository.Containers.AddAsync(container, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        await _cache.SetAsync($"{nameof(Container)}:{container.ContainerId}", container);

        await _eventHub.PublishAsync(new ContainerCreatedEvent
        {
            ContainerId = container.ContainerId
        });

        var dto = new ContainerDto(container);
        return dto;
    }
}
