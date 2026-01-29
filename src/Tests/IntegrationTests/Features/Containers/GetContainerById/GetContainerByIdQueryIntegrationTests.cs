using Application.Features.Containers.CreateContainer;
using Application.Features.Containers.GetContainerById;
using Bogus;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Features.Containers.GetContainerById;

[TestFixture]
public class GetContainerByIdQueryIntegrationTests
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
    public async Task HandleAsync_WhenContainerDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IGetContainerByIdQueryHandler>();

        var query = new GetContainerByIdQuery { ContainerId = 999999 };

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task HandleAsync_WhenContainerExists_ShouldReturnContainer()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var createHandler = scope.ServiceProvider.GetRequiredService<ICreateContainerCommandHandler>();
        var getByIdHandler = scope.ServiceProvider.GetRequiredService<IGetContainerByIdQueryHandler>();

        // Create a container first
        var createCommand = new CreateContainerCommand
        {
            Name = _faker.Commerce.ProductName(),
            Description = _faker.Lorem.Sentence()
        };
        var createdContainer = await createHandler.HandleAsync(createCommand, CancellationToken.None);

        var query = new GetContainerByIdQuery { ContainerId = createdContainer.ContainerId };

        // Act
        var result = await getByIdHandler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ContainerId.ShouldBe(createdContainer.ContainerId);
        result.Name.ShouldBe(createdContainer.Name);
        result.Description.ShouldBe(createdContainer.Description);
    }
}
