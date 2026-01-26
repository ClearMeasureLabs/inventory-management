using Application.Features.Containers;
using Application.Features.Containers.CreateContainer;
using Application.Infrastructure;
using Bogus;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ValidationException = Application.Exceptions.ValidationException;

namespace UnitTests.Features.Containers.CreateContainer;

[TestFixture]
public class CreateContainerCommandHandlerTests
{
    private Mock<IRepository> _repositoryMock = null!;
    private Mock<ICache> _cacheMock = null!;
    private Mock<IEventHub> _eventHubMock = null!;
    private Mock<DbSet<Container>> _containersDbSetMock = null!;
    private CreateContainerCommandHandler _handler = null!;
    private Faker _faker = null!;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<IRepository>();
        _cacheMock = new Mock<ICache>();
        _eventHubMock = new Mock<IEventHub>();
        _containersDbSetMock = new Mock<DbSet<Container>>();
        _faker = new Faker();

        _repositoryMock.Setup(r => r.Containers).Returns(_containersDbSetMock.Object);

        _handler = new CreateContainerCommandHandler(
            _repositoryMock.Object,
            _cacheMock.Object,
            _eventHubMock.Object);
    }

    #region Expected Dependencies Are Called

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldAddContainerToRepository()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _containersDbSetMock.Verify(
            db => db.AddAsync(It.Is<Container>(c => 
                c.Name == command.Name && 
                c.Description == command.Description), 
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldSaveChanges()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldCacheContainer()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _cacheMock.Verify(
            c => c.SetAsync(
                It.Is<string>(key => key.StartsWith("Container:")),
                It.Is<Container>(container => 
                    container.Name == command.Name && 
                    container.Description == command.Description),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldPublishContainerCreatedEvent()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _eventHubMock.Verify(
            e => e.PublishAsync(
                It.IsAny<ContainerCreatedEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldCallDependenciesInCorrectOrder()
    {
        // Arrange
        var command = CreateValidCommand();
        var callOrder = new List<string>();

        _containersDbSetMock
            .Setup(db => db.AddAsync(It.IsAny<Container>(), It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("AddAsync"));

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("SaveChangesAsync"))
            .ReturnsAsync(1);

        _cacheMock
            .Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<Container>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("SetAsync"));

        _eventHubMock
            .Setup(e => e.PublishAsync(It.IsAny<ContainerCreatedEvent>(), It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("PublishAsync"));

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        callOrder.ShouldBe(new[] { "AddAsync", "SaveChangesAsync", "SetAsync", "PublishAsync" });
    }

    #endregion

    #region Expected Business Logic Executed

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldCreateContainerWithCorrectName()
    {
        // Arrange
        var command = CreateValidCommand();
        Container? capturedContainer = null;

        _containersDbSetMock
            .Setup(db => db.AddAsync(It.IsAny<Container>(), It.IsAny<CancellationToken>()))
            .Callback<Container, CancellationToken>((container, _) => capturedContainer = container);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        capturedContainer.ShouldNotBeNull();
        capturedContainer.Name.ShouldBe(command.Name);
    }

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldCreateContainerWithCorrectDescription()
    {
        // Arrange
        var command = CreateValidCommand();
        Container? capturedContainer = null;

        _containersDbSetMock
            .Setup(db => db.AddAsync(It.IsAny<Container>(), It.IsAny<CancellationToken>()))
            .Callback<Container, CancellationToken>((container, _) => capturedContainer = container);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        capturedContainer.ShouldNotBeNull();
        capturedContainer.Description.ShouldBe(command.Description);
    }

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldPublishEventWithContainerId()
    {
        // Arrange
        var command = CreateValidCommand();
        ContainerCreatedEvent? capturedEvent = null;

        _eventHubMock
            .Setup(e => e.PublishAsync(It.IsAny<ContainerCreatedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<ContainerCreatedEvent, CancellationToken>((evt, _) => capturedEvent = evt);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        capturedEvent.ShouldNotBeNull();
        capturedEvent.ContainerId.ShouldBeGreaterThanOrEqualTo(0);
    }

    #endregion

    #region Expected State Changes Are Made and Persisted

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldPersistContainerBeforeCaching()
    {
        // Arrange
        var command = CreateValidCommand();
        var saveChangesCalled = false;
        var cacheCalledBeforeSave = false;

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => saveChangesCalled = true)
            .ReturnsAsync(1);

        _cacheMock
            .Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<Container>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .Callback(() => cacheCalledBeforeSave = !saveChangesCalled);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        cacheCalledBeforeSave.ShouldBeFalse("Cache should be called after SaveChangesAsync");
    }

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldCacheWithCorrectKey()
    {
        // Arrange
        var command = CreateValidCommand();
        string? capturedCacheKey = null;

        _cacheMock
            .Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<Container>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .Callback<string, Container, TimeSpan?, CancellationToken>((key, _, _, _) => capturedCacheKey = key);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        capturedCacheKey.ShouldNotBeNull();
        capturedCacheKey.ShouldStartWith("Container:");
    }

    #endregion

    #region Expected Return Values Are Present

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldReturnContainerDto()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<ContainerDto>();
    }

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldReturnDtoWithCorrectName()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Name.ShouldBe(command.Name);
    }

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldReturnDtoWithCorrectDescription()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Description.ShouldBe(command.Description);
    }

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldReturnDtoWithEmptyInventoryItems()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.InventoryItems.ShouldBeEmpty();
    }

    #endregion

    #region Expected Exceptions Are Thrown

    [Test]
    public void HandleAsync_WithEmptyName_ShouldThrowValidationException()
    {
        // Arrange
        var command = new CreateContainerCommand
        {
            Name = string.Empty,
            Description = _faker.Lorem.Sentence()
        };

        // Act & Assert
        var exception = Should.Throw<ValidationException>(async () =>
            await _handler.HandleAsync(command, CancellationToken.None));

        exception.Errors.ShouldContainKey("Name");
        exception.Errors["Name"].ShouldContain("Name is required");
    }

    [Test]
    public void HandleAsync_WithWhitespaceName_ShouldThrowValidationException()
    {
        // Arrange
        var command = new CreateContainerCommand
        {
            Name = "   ",
            Description = _faker.Lorem.Sentence()
        };

        // Act & Assert
        var exception = Should.Throw<ValidationException>(async () =>
            await _handler.HandleAsync(command, CancellationToken.None));

        exception.Errors.ShouldContainKey("Name");
    }

    [Test]
    public void HandleAsync_WithNullName_ShouldThrowValidationException()
    {
        // Arrange
        var command = new CreateContainerCommand
        {
            Name = null!,
            Description = _faker.Lorem.Sentence()
        };

        // Act & Assert
        var exception = Should.Throw<ValidationException>(async () =>
            await _handler.HandleAsync(command, CancellationToken.None));

        exception.Errors.ShouldContainKey("Name");
    }

    [Test]
    public async Task HandleAsync_WithEmptyDescription_ShouldSucceed()
    {
        // Arrange
        var command = new CreateContainerCommand
        {
            Name = _faker.Commerce.ProductName(),
            Description = string.Empty
        };

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(command.Name);
    }

    [Test]
    public async Task HandleAsync_WithValidNameOnly_ShouldCreateContainer()
    {
        // Arrange
        var command = new CreateContainerCommand
        {
            Name = _faker.Commerce.ProductName()
        };

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(command.Name);
    }

    [Test]
    public void HandleAsync_WithNameExceedingMaxLength_ShouldThrowValidationException()
    {
        // Arrange
        var command = new CreateContainerCommand
        {
            Name = new string('a', 201), // 201 characters exceeds max of 200
            Description = _faker.Lorem.Sentence()
        };

        // Act & Assert
        var exception = Should.Throw<ValidationException>(async () =>
            await _handler.HandleAsync(command, CancellationToken.None));

        exception.Errors.ShouldContainKey("Name");
        exception.Errors["Name"].ShouldContain("Name cannot exceed 200 characters");
    }

    [Test]
    public async Task HandleAsync_WithNameAtMaxLength_ShouldSucceed()
    {
        // Arrange
        var command = new CreateContainerCommand
        {
            Name = new string('a', 200), // Exactly 200 characters
            Description = _faker.Lorem.Sentence()
        };

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(command.Name);
    }

    [Test]
    public void HandleAsync_WithDescriptionExceedingMaxLength_ShouldThrowValidationException()
    {
        // Arrange
        var command = new CreateContainerCommand
        {
            Name = _faker.Commerce.ProductName(),
            Description = new string('a', 251) // 251 characters exceeds max of 250
        };

        // Act & Assert
        var exception = Should.Throw<ValidationException>(async () =>
            await _handler.HandleAsync(command, CancellationToken.None));

        exception.Errors.ShouldContainKey("Description");
        exception.Errors["Description"].ShouldContain("Description cannot exceed 250 characters");
    }

    [Test]
    public async Task HandleAsync_WithDescriptionAtMaxLength_ShouldSucceed()
    {
        // Arrange
        var command = new CreateContainerCommand
        {
            Name = _faker.Commerce.ProductName(),
            Description = new string('a', 250) // Exactly 250 characters
        };

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Description.ShouldBe(command.Description);
    }

    [Test]
    public async Task HandleAsync_WithDescriptionHavingLeadingAndTrailingWhitespace_ShouldTrimWhitespace()
    {
        // Arrange
        var descriptionContent = "Test description content";
        var command = new CreateContainerCommand
        {
            Name = _faker.Commerce.ProductName(),
            Description = $"   {descriptionContent}   "
        };
        Container? capturedContainer = null;

        _containersDbSetMock
            .Setup(db => db.AddAsync(It.IsAny<Container>(), It.IsAny<CancellationToken>()))
            .Callback<Container, CancellationToken>((container, _) => capturedContainer = container);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        capturedContainer.ShouldNotBeNull();
        capturedContainer.Description.ShouldBe(descriptionContent);
        result.Description.ShouldBe(descriptionContent);
    }

    [Test]
    public async Task HandleAsync_WithNullDescription_ShouldSucceed()
    {
        // Arrange
        var command = new CreateContainerCommand
        {
            Name = _faker.Commerce.ProductName(),
            Description = null!
        };

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Description.ShouldBe(string.Empty);
    }

    [Test]
    public async Task HandleAsync_WithWhitespaceOnlyDescription_ShouldTrimToEmpty()
    {
        // Arrange
        var command = new CreateContainerCommand
        {
            Name = _faker.Commerce.ProductName(),
            Description = "   "
        };
        Container? capturedContainer = null;

        _containersDbSetMock
            .Setup(db => db.AddAsync(It.IsAny<Container>(), It.IsAny<CancellationToken>()))
            .Callback<Container, CancellationToken>((container, _) => capturedContainer = container);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        capturedContainer.ShouldNotBeNull();
        capturedContainer.Description.ShouldBe(string.Empty);
        result.Description.ShouldBe(string.Empty);
    }

    [Test]
    public void HandleAsync_WithInvalidCommand_ShouldNotCallRepository()
    {
        // Arrange
        var command = new CreateContainerCommand
        {
            Name = string.Empty // Only Name is required now
        };

        // Act & Assert
        Should.Throw<ValidationException>(async () =>
            await _handler.HandleAsync(command, CancellationToken.None));

        _containersDbSetMock.Verify(
            db => db.AddAsync(It.IsAny<Container>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _repositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public void HandleAsync_WithInvalidCommand_ShouldNotCallCache()
    {
        // Arrange
        var command = new CreateContainerCommand
        {
            Name = string.Empty // Only Name is required now
        };

        // Act & Assert
        Should.Throw<ValidationException>(async () =>
            await _handler.HandleAsync(command, CancellationToken.None));

        _cacheMock.Verify(
            c => c.SetAsync(It.IsAny<string>(), It.IsAny<Container>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public void HandleAsync_WithInvalidCommand_ShouldNotPublishEvent()
    {
        // Arrange
        var command = new CreateContainerCommand
        {
            Name = string.Empty // Only Name is required now
        };

        // Act & Assert
        Should.Throw<ValidationException>(async () =>
            await _handler.HandleAsync(command, CancellationToken.None));

        _eventHubMock.Verify(
            e => e.PublishAsync(It.IsAny<ContainerCreatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Helper Methods

    private CreateContainerCommand CreateValidCommand()
    {
        return new CreateContainerCommand
        {
            Name = _faker.Commerce.ProductName(),
            Description = _faker.Lorem.Sentence()
        };
    }

    #endregion
}
