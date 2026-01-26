using Application.Features.Containers.DeleteContainer;
using Application.Infrastructure;
using Bogus;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using ValidationException = Application.Exceptions.ValidationException;

namespace UnitTests.Features.Containers.DeleteContainer;

[TestFixture]
public class DeleteContainerCommandHandlerTests
{
    private Mock<IRepository> _repositoryMock = null!;
    private Mock<ICache> _cacheMock = null!;
    private Mock<IEventHub> _eventHubMock = null!;
    private DeleteContainerCommandHandler _handler = null!;
    private Faker _faker = null!;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<IRepository>();
        _cacheMock = new Mock<ICache>();
        _eventHubMock = new Mock<IEventHub>();
        _faker = new Faker();

        _handler = new DeleteContainerCommandHandler(
            _repositoryMock.Object,
            _cacheMock.Object,
            _eventHubMock.Object);
    }

    #region Expected Dependencies Are Called

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldRemoveContainerFromRepository()
    {
        // Arrange
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = new DeleteContainerCommand { ContainerId = container.ContainerId };

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            r => r.Containers.Remove(It.Is<Container>(c => c.ContainerId == container.ContainerId)),
            Times.Once);
    }

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldSaveChanges()
    {
        // Arrange
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = new DeleteContainerCommand { ContainerId = container.ContainerId };

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldRemoveFromCache()
    {
        // Arrange
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = new DeleteContainerCommand { ContainerId = container.ContainerId };

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _cacheMock.Verify(
            c => c.RemoveAsync(
                It.Is<string>(key => key == $"Container:{container.ContainerId}"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldPublishContainerDeletedEvent()
    {
        // Arrange
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = new DeleteContainerCommand { ContainerId = container.ContainerId };

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _eventHubMock.Verify(
            e => e.PublishAsync(
                It.Is<ContainerDeletedEvent>(evt => evt.ContainerId == container.ContainerId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldCallDependenciesInCorrectOrder()
    {
        // Arrange
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = new DeleteContainerCommand { ContainerId = container.ContainerId };
        var callOrder = new List<string>();

        _repositoryMock
            .Setup(r => r.Containers.Remove(It.IsAny<Container>()))
            .Callback(() => callOrder.Add("Remove"));

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("SaveChangesAsync"))
            .ReturnsAsync(1);

        _cacheMock
            .Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("RemoveAsync"));

        _eventHubMock
            .Setup(e => e.PublishAsync(It.IsAny<ContainerDeletedEvent>(), It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("PublishAsync"));

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        callOrder.ShouldBe(new[] { "Remove", "SaveChangesAsync", "RemoveAsync", "PublishAsync" });
    }

    #endregion

    #region Expected Business Logic Executed

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldPublishEventWithCorrectContainerId()
    {
        // Arrange
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = new DeleteContainerCommand { ContainerId = container.ContainerId };
        ContainerDeletedEvent? capturedEvent = null;

        _eventHubMock
            .Setup(e => e.PublishAsync(It.IsAny<ContainerDeletedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<ContainerDeletedEvent, CancellationToken>((evt, _) => capturedEvent = evt);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        capturedEvent.ShouldNotBeNull();
        capturedEvent.ContainerId.ShouldBe(container.ContainerId);
    }

    #endregion

    #region Expected State Changes Are Made and Persisted

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldRemoveFromCacheWithCorrectKey()
    {
        // Arrange
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = new DeleteContainerCommand { ContainerId = container.ContainerId };
        string? capturedCacheKey = null;

        _cacheMock
            .Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((key, _) => capturedCacheKey = key);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        capturedCacheKey.ShouldNotBeNull();
        capturedCacheKey.ShouldBe($"Container:{container.ContainerId}");
    }

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldPersistRemovalBeforeCacheClearing()
    {
        // Arrange
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = new DeleteContainerCommand { ContainerId = container.ContainerId };
        var saveChangesCalled = false;
        var cacheCalledBeforeSave = false;

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => saveChangesCalled = true)
            .ReturnsAsync(1);

        _cacheMock
            .Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback(() => cacheCalledBeforeSave = !saveChangesCalled);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        cacheCalledBeforeSave.ShouldBeFalse("Cache should be cleared after SaveChangesAsync");
    }

    #endregion

    #region Expected Exceptions Are Thrown

    [Test]
    public void HandleAsync_WithNonExistentContainer_ShouldThrowValidationException()
    {
        // Arrange
        SetupContainersDbSet(new List<Container>());

        var command = new DeleteContainerCommand { ContainerId = 999 };

        // Act & Assert
        var exception = Should.Throw<ValidationException>(async () =>
            await _handler.HandleAsync(command, CancellationToken.None));

        exception.Errors.ShouldContainKey("ContainerId");
        exception.Errors["ContainerId"].ShouldContain("Container not found");
    }

    [Test]
    public void HandleAsync_WithContainerThatHasItems_ShouldThrowValidationException()
    {
        // Arrange
        var container = CreateValidContainer();
        container.InventoryItems = new List<ContainerItem>
        {
            new ContainerItem { ContainerItemId = 1, ContainerId = container.ContainerId, ItemId = 1 }
        };
        SetupContainersDbSet(new List<Container> { container });

        var command = new DeleteContainerCommand { ContainerId = container.ContainerId };

        // Act & Assert
        var exception = Should.Throw<ValidationException>(async () =>
            await _handler.HandleAsync(command, CancellationToken.None));

        exception.Errors.ShouldContainKey("ContainerId");
        exception.Errors["ContainerId"].ShouldContain("Cannot delete a container that has items");
    }

    [Test]
    public void HandleAsync_WithNonExistentContainer_ShouldNotCallRepository()
    {
        // Arrange
        SetupContainersDbSet(new List<Container>());

        var command = new DeleteContainerCommand { ContainerId = 999 };

        // Act & Assert
        Should.Throw<ValidationException>(async () =>
            await _handler.HandleAsync(command, CancellationToken.None));

        _repositoryMock.Verify(
            r => r.Containers.Remove(It.IsAny<Container>()),
            Times.Never);
        _repositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public void HandleAsync_WithContainerThatHasItems_ShouldNotCallRepository()
    {
        // Arrange
        var container = CreateValidContainer();
        container.InventoryItems = new List<ContainerItem>
        {
            new ContainerItem { ContainerItemId = 1, ContainerId = container.ContainerId, ItemId = 1 }
        };
        SetupContainersDbSet(new List<Container> { container });

        var command = new DeleteContainerCommand { ContainerId = container.ContainerId };

        // Act & Assert
        Should.Throw<ValidationException>(async () =>
            await _handler.HandleAsync(command, CancellationToken.None));

        _repositoryMock.Verify(
            r => r.Containers.Remove(It.IsAny<Container>()),
            Times.Never);
        _repositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public void HandleAsync_WithNonExistentContainer_ShouldNotCallCache()
    {
        // Arrange
        SetupContainersDbSet(new List<Container>());

        var command = new DeleteContainerCommand { ContainerId = 999 };

        // Act & Assert
        Should.Throw<ValidationException>(async () =>
            await _handler.HandleAsync(command, CancellationToken.None));

        _cacheMock.Verify(
            c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public void HandleAsync_WithNonExistentContainer_ShouldNotPublishEvent()
    {
        // Arrange
        SetupContainersDbSet(new List<Container>());

        var command = new DeleteContainerCommand { ContainerId = 999 };

        // Act & Assert
        Should.Throw<ValidationException>(async () =>
            await _handler.HandleAsync(command, CancellationToken.None));

        _eventHubMock.Verify(
            e => e.PublishAsync(It.IsAny<ContainerDeletedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Helper Methods

    private Container CreateValidContainer()
    {
        return new Container
        {
            ContainerId = _faker.Random.Int(1, 1000),
            Name = _faker.Commerce.ProductName(),
            Description = _faker.Lorem.Sentence(),
            InventoryItems = new List<ContainerItem>()
        };
    }

    private void SetupContainersDbSet(List<Container> containers)
    {
        var mockDbSet = containers.AsQueryable().BuildMockDbSet();
        _repositoryMock.Setup(r => r.Containers).Returns(mockDbSet.Object);
    }

    #endregion
}
