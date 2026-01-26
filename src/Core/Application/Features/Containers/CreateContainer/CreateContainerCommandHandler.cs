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

    private const int NameMaxLength = 200;
    private const int DescriptionMaxLength = 250;

    public async Task<ContainerDto> HandleAsync(CreateContainerCommand request, CancellationToken cancellationToken)
    {
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

        var trimmedDescription = request.Description?.Trim() ?? string.Empty;
        if (trimmedDescription.Length > DescriptionMaxLength)
        {
            errors.TryAdd(nameof(request.Description), new List<string>());
            errors[nameof(request.Description)].Add($"Description cannot exceed {DescriptionMaxLength} characters");
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
            Description = trimmedDescription
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
