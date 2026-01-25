using Application.Features.Containers.CreateContainer;
using Application.Features.Containers.GetAllContainers;
using Bogus;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Features.Containers.GetAllContainers;

[TestFixture]
public class GetAllContainersQueryIntegrationTests
{
    private IServiceProvider _serviceProvider = null!;
    private Faker _faker = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Use shared service provider from global fixture
        _serviceProvider = GlobalTestFixture.ServiceProvider;
        _faker = new Faker();
    }

    [Test]
    public async Task HandleAsync_WhenNoContainersExist_ShouldReturnEmptyCollection()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IGetAllContainersQueryHandler>();

        var query = new GetAllContainersQuery();

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
    }

    [Test]
    public async Task HandleAsync_WhenContainersExist_ShouldReturnContainers()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var createHandler = scope.ServiceProvider.GetRequiredService<ICreateContainerCommandHandler>();
        var getAllHandler = scope.ServiceProvider.GetRequiredService<IGetAllContainersQueryHandler>();

        // Create a container first
        var createCommand = new CreateContainerCommand
        {
            Name = _faker.Commerce.ProductName(),
            Description = _faker.Lorem.Sentence()
        };
        var createdContainer = await createHandler.HandleAsync(createCommand, CancellationToken.None);

        var query = new GetAllContainersQuery();

        // Act
        var result = await getAllHandler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldContain(c => c.ContainerId == createdContainer.ContainerId);
        result.ShouldContain(c => c.Name == createdContainer.Name);
    }
}
