using Application.Features.Containers.CreateContainer;
using Application.Features.Containers.UpdateContainer;
using Application.Infrastructure;
using Bogus;
using Microsoft.EntityFrameworkCore;
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
    public async Task HandleAsync_WithExistingContainer_ShouldUpdateSuccessfully()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var createHandler = scope.ServiceProvider.GetRequiredService<ICreateContainerCommandHandler>();
        var updateHandler = scope.ServiceProvider.GetRequiredService<IUpdateContainerCommandHandler>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();

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
        result.ShouldNotBeNull();
        result.ContainerId.ShouldBe(createdContainer.ContainerId);
        result.Name.ShouldBe(newName);
        result.Description.ShouldBe(newDescription);

        // Verify in database
        var containerInDb = await repository.Containers
            .FirstOrDefaultAsync(c => c.ContainerId == createdContainer.ContainerId);
        containerInDb.ShouldNotBeNull();
        containerInDb.Name.ShouldBe(newName);
        containerInDb.Description.ShouldBe(newDescription);
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
        var createHandler = scope.ServiceProvider.GetRequiredService<ICreateContainerCommandHandler>();
        var updateHandler = scope.ServiceProvider.GetRequiredService<IUpdateContainerCommandHandler>();

        // First create a container
        var createCommand = new CreateContainerCommand
        {
            Name = _faker.Commerce.ProductName(),
            Description = _faker.Lorem.Sentence()
        };
        var createdContainer = createHandler.HandleAsync(createCommand, CancellationToken.None).GetAwaiter().GetResult();

        var updateCommand = new UpdateContainerCommand
        {
            ContainerId = createdContainer.ContainerId,
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
    public void HandleAsync_WithDuplicateName_ShouldThrowValidationException()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var createHandler = scope.ServiceProvider.GetRequiredService<ICreateContainerCommandHandler>();
        var updateHandler = scope.ServiceProvider.GetRequiredService<IUpdateContainerCommandHandler>();

        // Create first container with a specific name
        var duplicateName = $"Duplicate-{_faker.Random.Guid()}";
        var createCommand1 = new CreateContainerCommand
        {
            Name = duplicateName,
            Description = _faker.Lorem.Sentence()
        };
        createHandler.HandleAsync(createCommand1, CancellationToken.None).GetAwaiter().GetResult();

        // Create second container with a different name
        var createCommand2 = new CreateContainerCommand
        {
            Name = _faker.Commerce.ProductName(),
            Description = _faker.Lorem.Sentence()
        };
        var container2 = createHandler.HandleAsync(createCommand2, CancellationToken.None).GetAwaiter().GetResult();

        // Try to update second container with the duplicate name
        var updateCommand = new UpdateContainerCommand
        {
            ContainerId = container2.ContainerId,
            Name = duplicateName,
            Description = _faker.Lorem.Sentence()
        };

        // Act & Assert
        var exception = Should.Throw<ValidationException>(async () =>
            await updateHandler.HandleAsync(updateCommand, CancellationToken.None));

        exception.Errors.ShouldContainKey("Name");
        exception.Errors["Name"].ShouldContain("A container with this name already exists");
    }

    [Test]
    public async Task HandleAsync_WithSameNameOnSameContainer_ShouldSucceed()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var createHandler = scope.ServiceProvider.GetRequiredService<ICreateContainerCommandHandler>();
        var updateHandler = scope.ServiceProvider.GetRequiredService<IUpdateContainerCommandHandler>();

        // Create a container
        var originalName = $"Original-{_faker.Random.Guid()}";
        var createCommand = new CreateContainerCommand
        {
            Name = originalName,
            Description = _faker.Lorem.Sentence()
        };
        var createdContainer = await createHandler.HandleAsync(createCommand, CancellationToken.None);

        // Update with the same name (only changing description)
        var newDescription = _faker.Lorem.Sentence();
        var updateCommand = new UpdateContainerCommand
        {
            ContainerId = createdContainer.ContainerId,
            Name = originalName, // Same name
            Description = newDescription
        };

        // Act
        var result = await updateHandler.HandleAsync(updateCommand, CancellationToken.None);

        // Assert - should succeed without duplicate name error
        result.ShouldNotBeNull();
        result.Name.ShouldBe(originalName);
        result.Description.ShouldBe(newDescription);
    }

    [Test]
    public async Task HandleAsync_WithUpdatedNameOnly_ShouldPreserveDescription()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var createHandler = scope.ServiceProvider.GetRequiredService<ICreateContainerCommandHandler>();
        var updateHandler = scope.ServiceProvider.GetRequiredService<IUpdateContainerCommandHandler>();

        var originalDescription = _faker.Lorem.Sentence();
        var createCommand = new CreateContainerCommand
        {
            Name = _faker.Commerce.ProductName(),
            Description = originalDescription
        };
        var createdContainer = await createHandler.HandleAsync(createCommand, CancellationToken.None);

        var newName = _faker.Commerce.ProductName();
        var updateCommand = new UpdateContainerCommand
        {
            ContainerId = createdContainer.ContainerId,
            Name = newName,
            Description = originalDescription // Keep same description
        };

        // Act
        var result = await updateHandler.HandleAsync(updateCommand, CancellationToken.None);

        // Assert
        result.Name.ShouldBe(newName);
        result.Description.ShouldBe(originalDescription);
    }

    [Test]
    public async Task HandleAsync_WithEmptyDescription_ShouldClearDescription()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var createHandler = scope.ServiceProvider.GetRequiredService<ICreateContainerCommandHandler>();
        var updateHandler = scope.ServiceProvider.GetRequiredService<IUpdateContainerCommandHandler>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();

        var createCommand = new CreateContainerCommand
        {
            Name = _faker.Commerce.ProductName(),
            Description = _faker.Lorem.Sentence()
        };
        var createdContainer = await createHandler.HandleAsync(createCommand, CancellationToken.None);

        var updateCommand = new UpdateContainerCommand
        {
            ContainerId = createdContainer.ContainerId,
            Name = _faker.Commerce.ProductName(),
            Description = string.Empty
        };

        // Act
        var result = await updateHandler.HandleAsync(updateCommand, CancellationToken.None);

        // Assert
        result.Description.ShouldBe(string.Empty);

        // Verify in database
        var containerInDb = await repository.Containers
            .FirstOrDefaultAsync(c => c.ContainerId == createdContainer.ContainerId);
        containerInDb.ShouldNotBeNull();
        containerInDb.Description.ShouldBe(string.Empty);
    }
}
