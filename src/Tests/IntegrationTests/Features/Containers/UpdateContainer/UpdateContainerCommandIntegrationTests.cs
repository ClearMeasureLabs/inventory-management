using Application.Features.Containers.CreateContainer;
using Application.Features.Containers.UpdateContainer;
using Bogus;
using Microsoft.Extensions.DependencyInjection;
using ValidationException = Application.Exceptions.ValidationException;

namespace IntegrationTests.Features.Containers.UpdateContainer;

[TestFixture]
public class UpdateContainerCommandIntegrationTests
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
    public async Task HandleAsync_WithValidCommand_ShouldSucceedWithoutErrors()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var createHandler = scope.ServiceProvider.GetRequiredService<ICreateContainerCommandHandler>();
        var updateHandler = scope.ServiceProvider.GetRequiredService<IUpdateContainerCommandHandler>();

        // First create a container
        var createCommand = new CreateContainerCommand
        {
            Name = _faker.Commerce.ProductName(),
            Description = _faker.Lorem.Sentence()
        };
        var createdContainer = await createHandler.HandleAsync(createCommand, CancellationToken.None);

        // Then update it
        var updateCommand = new UpdateContainerCommand
        {
            ContainerId = createdContainer.ContainerId,
            Name = _faker.Commerce.ProductName(),
            Description = _faker.Lorem.Sentence()
        };

        // Act
        var result = await updateHandler.HandleAsync(updateCommand, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ContainerId.ShouldBe(createdContainer.ContainerId);
        result.Name.ShouldBe(updateCommand.Name);
        result.Description.ShouldBe(updateCommand.Description);
    }

    [Test]
    public async Task HandleAsync_WithUpdatedNameAndDescription_ShouldPersistChanges()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var createHandler = scope.ServiceProvider.GetRequiredService<ICreateContainerCommandHandler>();
        var updateHandler = scope.ServiceProvider.GetRequiredService<IUpdateContainerCommandHandler>();

        // First create a container
        var createCommand = new CreateContainerCommand
        {
            Name = _faker.Commerce.ProductName(),
            Description = _faker.Lorem.Sentence()
        };
        var createdContainer = await createHandler.HandleAsync(createCommand, CancellationToken.None);

        var newName = _faker.Commerce.ProductName();
        var newDescription = _faker.Lorem.Sentence();

        var updateCommand = new UpdateContainerCommand
        {
            ContainerId = createdContainer.ContainerId,
            Name = newName,
            Description = newDescription
        };

        // Act
        var result = await updateHandler.HandleAsync(updateCommand, CancellationToken.None);

        // Assert
        result.Name.ShouldBe(newName);
        result.Description.ShouldBe(newDescription);
    }

    [Test]
    public void HandleAsync_WithNonExistentContainer_ShouldThrowValidationException()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var updateHandler = scope.ServiceProvider.GetRequiredService<IUpdateContainerCommandHandler>();

        var updateCommand = new UpdateContainerCommand
        {
            ContainerId = 999999, // Non-existent ID
            Name = _faker.Commerce.ProductName(),
            Description = _faker.Lorem.Sentence()
        };

        // Act & Assert
        var exception = Should.Throw<ValidationException>(async () =>
            await updateHandler.HandleAsync(updateCommand, CancellationToken.None));

        exception.Errors.ShouldContainKey("ContainerId");
        exception.Errors["ContainerId"].ShouldContain("Container not found");
    }

    [Test]
    public void HandleAsync_WithEmptyName_ShouldThrowValidationException()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var updateHandler = scope.ServiceProvider.GetRequiredService<IUpdateContainerCommandHandler>();

        var updateCommand = new UpdateContainerCommand
        {
            ContainerId = 1,
            Name = string.Empty,
            Description = _faker.Lorem.Sentence()
        };

        // Act & Assert
        var exception = Should.Throw<ValidationException>(async () =>
            await updateHandler.HandleAsync(updateCommand, CancellationToken.None));

        exception.Errors.ShouldContainKey("Name");
        exception.Errors["Name"].ShouldContain("Name is required");
    }

    [Test]
    public async Task HandleAsync_WithDuplicateName_ShouldThrowValidationException()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var createHandler = scope.ServiceProvider.GetRequiredService<ICreateContainerCommandHandler>();
        var updateHandler = scope.ServiceProvider.GetRequiredService<IUpdateContainerCommandHandler>();

        // Create first container
        var firstContainerName = _faker.Commerce.ProductName() + " " + _faker.Random.Guid().ToString();
        var createCommand1 = new CreateContainerCommand
        {
            Name = firstContainerName,
            Description = _faker.Lorem.Sentence()
        };
        await createHandler.HandleAsync(createCommand1, CancellationToken.None);

        // Create second container
        var createCommand2 = new CreateContainerCommand
        {
            Name = _faker.Commerce.ProductName() + " " + _faker.Random.Guid().ToString(),
            Description = _faker.Lorem.Sentence()
        };
        var secondContainer = await createHandler.HandleAsync(createCommand2, CancellationToken.None);

        // Try to update second container with first container's name
        var updateCommand = new UpdateContainerCommand
        {
            ContainerId = secondContainer.ContainerId,
            Name = firstContainerName, // Duplicate name
            Description = _faker.Lorem.Sentence()
        };

        // Act & Assert
        var exception = Should.Throw<ValidationException>(async () =>
            await updateHandler.HandleAsync(updateCommand, CancellationToken.None));

        exception.Errors.ShouldContainKey("Name");
        exception.Errors["Name"].ShouldContain("A container with this name already exists");
    }
}
