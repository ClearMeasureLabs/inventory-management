using Application.Features.Containers.CreateContainer;
using Application.Features.Containers.DeleteContainer;
using Application.Infrastructure;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ValidationException = Application.Exceptions.ValidationException;

namespace IntegrationTests.Features.Containers.DeleteContainer;

[TestFixture]
public class DeleteContainerCommandIntegrationTests
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
    public async Task HandleAsync_WithExistingEmptyContainer_ShouldSucceedWithoutErrors()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var createHandler = scope.ServiceProvider.GetRequiredService<ICreateContainerCommandHandler>();
        var deleteHandler = scope.ServiceProvider.GetRequiredService<IDeleteContainerCommandHandler>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();

        // First create a container
        var createCommand = new CreateContainerCommand
        {
            Name = _faker.Commerce.ProductName(),
            Description = _faker.Lorem.Sentence()
        };
        var createdContainer = await createHandler.HandleAsync(createCommand, CancellationToken.None);

        var deleteCommand = new DeleteContainerCommand
        {
            ContainerId = createdContainer.ContainerId
        };

        // Act
        await deleteHandler.HandleAsync(deleteCommand, CancellationToken.None);

        // Assert
        var containerExists = await repository.Containers
            .AnyAsync(c => c.ContainerId == createdContainer.ContainerId);
        containerExists.ShouldBeFalse();
    }

    [Test]
    public void HandleAsync_WithNonExistentContainer_ShouldThrowValidationException()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var deleteHandler = scope.ServiceProvider.GetRequiredService<IDeleteContainerCommandHandler>();

        var deleteCommand = new DeleteContainerCommand
        {
            ContainerId = 999999 // Non-existent ID
        };

        // Act & Assert
        var exception = Should.Throw<ValidationException>(async () =>
            await deleteHandler.HandleAsync(deleteCommand, CancellationToken.None));

        exception.Errors.ShouldContainKey("ContainerId");
        exception.Errors["ContainerId"].ShouldContain("Container not found");
    }
}
