using System.Linq.Expressions;
using Application.Features.Containers;
using Application.Features.Containers.UpdateContainer;
using Application.Infrastructure;
using Bogus;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using ValidationException = Application.Exceptions.ValidationException;

namespace UnitTests.Features.Containers.UpdateContainer;

[TestFixture]
public class UpdateContainerCommandHandlerTests
{
    private Mock<IRepository> _repositoryMock = null!;
    private Mock<ICache> _cacheMock = null!;
    private Mock<IEventHub> _eventHubMock = null!;
    private UpdateContainerCommandHandler _handler = null!;
    private Faker _faker = null!;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<IRepository>();
        _cacheMock = new Mock<ICache>();
        _eventHubMock = new Mock<IEventHub>();
        _faker = new Faker();

        _handler = new UpdateContainerCommandHandler(
            _repositoryMock.Object,
            _cacheMock.Object,
            _eventHubMock.Object);
    }

    #region Expected Dependencies Are Called

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldSaveChanges()
    {
        // Arrange
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = CreateValidCommand(container.ContainerId);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldUpdateCache()
    {
        // Arrange
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = CreateValidCommand(container.ContainerId);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _cacheMock.Verify(
            c => c.SetAsync(
                It.Is<string>(key => key == $"Container:{container.ContainerId}"),
                It.Is<Container>(c => c.Name == command.Name && c.Description == command.Description),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldPublishContainerUpdatedEvent()
    {
        // Arrange
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = CreateValidCommand(container.ContainerId);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _eventHubMock.Verify(
            e => e.PublishAsync(
                It.Is<ContainerUpdatedEvent>(evt => evt.ContainerId == container.ContainerId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldCallDependenciesInCorrectOrder()
    {
        // Arrange
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = CreateValidCommand(container.ContainerId);
        var callOrder = new List<string>();

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("SaveChangesAsync"))
            .ReturnsAsync(1);

        _cacheMock
            .Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<Container>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("SetAsync"));

        _eventHubMock
            .Setup(e => e.PublishAsync(It.IsAny<ContainerUpdatedEvent>(), It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("PublishAsync"));

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        callOrder.ShouldBe(new[] { "SaveChangesAsync", "SetAsync", "PublishAsync" });
    }

    #endregion

    #region Expected Business Logic Executed

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldUpdateContainerName()
    {
        // Arrange
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = CreateValidCommand(container.ContainerId);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        container.Name.ShouldBe(command.Name);
    }

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldUpdateContainerDescription()
    {
        // Arrange
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = CreateValidCommand(container.ContainerId);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        container.Description.ShouldBe(command.Description);
    }

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldPublishEventWithCorrectContainerId()
    {
        // Arrange
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = CreateValidCommand(container.ContainerId);
        ContainerUpdatedEvent? capturedEvent = null;

        _eventHubMock
            .Setup(e => e.PublishAsync(It.IsAny<ContainerUpdatedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<ContainerUpdatedEvent, CancellationToken>((evt, _) => capturedEvent = evt);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        capturedEvent.ShouldNotBeNull();
        capturedEvent.ContainerId.ShouldBe(container.ContainerId);
    }

    #endregion

    #region Expected State Changes Are Made and Persisted

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldCacheWithCorrectKey()
    {
        // Arrange
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = CreateValidCommand(container.ContainerId);
        string? capturedCacheKey = null;

        _cacheMock
            .Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<Container>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .Callback<string, Container, TimeSpan?, CancellationToken>((key, _, _, _) => capturedCacheKey = key);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        capturedCacheKey.ShouldNotBeNull();
        capturedCacheKey.ShouldBe($"Container:{container.ContainerId}");
    }

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldPersistBeforeCaching()
    {
        // Arrange
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = CreateValidCommand(container.ContainerId);
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
        cacheCalledBeforeSave.ShouldBeFalse("Cache should be updated after SaveChangesAsync");
    }

    #endregion

    #region Expected Return Values Are Present

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldReturnContainerDto()
    {
        // Arrange
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = CreateValidCommand(container.ContainerId);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<ContainerDto>();
    }

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldReturnDtoWithCorrectId()
    {
        // Arrange
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = CreateValidCommand(container.ContainerId);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.ContainerId.ShouldBe(container.ContainerId);
    }

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldReturnDtoWithUpdatedName()
    {
        // Arrange
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = CreateValidCommand(container.ContainerId);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Name.ShouldBe(command.Name);
    }

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldReturnDtoWithUpdatedDescription()
    {
        // Arrange
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = CreateValidCommand(container.ContainerId);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Description.ShouldBe(command.Description);
    }

    #endregion

    #region Expected Exceptions Are Thrown

    [Test]
    public void HandleAsync_WithEmptyName_ShouldThrowValidationException()
    {
        // Arrange
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = new UpdateContainerCommand
        {
            ContainerId = container.ContainerId,
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
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = new UpdateContainerCommand
        {
            ContainerId = container.ContainerId,
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
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = new UpdateContainerCommand
        {
            ContainerId = container.ContainerId,
            Name = null!,
            Description = _faker.Lorem.Sentence()
        };

        // Act & Assert
        var exception = Should.Throw<ValidationException>(async () =>
            await _handler.HandleAsync(command, CancellationToken.None));

        exception.Errors.ShouldContainKey("Name");
    }

    [Test]
    public void HandleAsync_WithNameExceedingMaxLength_ShouldThrowValidationException()
    {
        // Arrange
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = new UpdateContainerCommand
        {
            ContainerId = container.ContainerId,
            Name = new string('a', 501), // 501 characters exceeds max of 500
            Description = _faker.Lorem.Sentence()
        };

        // Act & Assert
        var exception = Should.Throw<ValidationException>(async () =>
            await _handler.HandleAsync(command, CancellationToken.None));

        exception.Errors.ShouldContainKey("Name");
        exception.Errors["Name"].ShouldContain("Name cannot exceed 500 characters");
    }

    [Test]
    public async Task HandleAsync_WithNameAtMaxLength_ShouldSucceed()
    {
        // Arrange
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = new UpdateContainerCommand
        {
            ContainerId = container.ContainerId,
            Name = new string('a', 500), // Exactly 500 characters
            Description = _faker.Lorem.Sentence()
        };

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(command.Name);
    }

    [Test]
    public void HandleAsync_WithNonExistentContainer_ShouldThrowValidationException()
    {
        // Arrange
        SetupContainersDbSet(new List<Container>());

        var command = new UpdateContainerCommand
        {
            ContainerId = 999,
            Name = _faker.Commerce.ProductName(),
            Description = _faker.Lorem.Sentence()
        };

        // Act & Assert
        var exception = Should.Throw<ValidationException>(async () =>
            await _handler.HandleAsync(command, CancellationToken.None));

        exception.Errors.ShouldContainKey("ContainerId");
        exception.Errors["ContainerId"].ShouldContain("Container not found");
    }

    [Test]
    public void HandleAsync_WithDuplicateName_ShouldThrowValidationException()
    {
        // Arrange
        var existingContainer = new Container
        {
            ContainerId = 1,
            Name = "Existing Container",
            Description = _faker.Lorem.Sentence()
        };
        var containerToUpdate = new Container
        {
            ContainerId = 2,
            Name = "Original Name",
            Description = _faker.Lorem.Sentence()
        };
        SetupContainersDbSet(new List<Container> { existingContainer, containerToUpdate });

        var command = new UpdateContainerCommand
        {
            ContainerId = containerToUpdate.ContainerId,
            Name = existingContainer.Name, // Try to use duplicate name
            Description = _faker.Lorem.Sentence()
        };

        // Act & Assert
        var exception = Should.Throw<ValidationException>(async () =>
            await _handler.HandleAsync(command, CancellationToken.None));

        exception.Errors.ShouldContainKey("Name");
        exception.Errors["Name"].ShouldContain("A container with this name already exists");
    }

    [Test]
    public async Task HandleAsync_WithSameNameAsCurrentContainer_ShouldSucceed()
    {
        // Arrange
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = new UpdateContainerCommand
        {
            ContainerId = container.ContainerId,
            Name = container.Name, // Same name as current
            Description = _faker.Lorem.Sentence()
        };

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(command.Name);
    }

    [Test]
    public async Task HandleAsync_WithEmptyDescription_ShouldSucceed()
    {
        // Arrange
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = new UpdateContainerCommand
        {
            ContainerId = container.ContainerId,
            Name = _faker.Commerce.ProductName(),
            Description = string.Empty
        };

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Description.ShouldBe(string.Empty);
    }

    [Test]
    public void HandleAsync_WithInvalidCommand_ShouldNotCallSaveChanges()
    {
        // Arrange
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = new UpdateContainerCommand
        {
            ContainerId = container.ContainerId,
            Name = string.Empty // Invalid name
        };

        // Act & Assert
        Should.Throw<ValidationException>(async () =>
            await _handler.HandleAsync(command, CancellationToken.None));

        _repositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public void HandleAsync_WithInvalidCommand_ShouldNotCallCache()
    {
        // Arrange
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = new UpdateContainerCommand
        {
            ContainerId = container.ContainerId,
            Name = string.Empty // Invalid name
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
        var container = CreateValidContainer();
        SetupContainersDbSet(new List<Container> { container });

        var command = new UpdateContainerCommand
        {
            ContainerId = container.ContainerId,
            Name = string.Empty // Invalid name
        };

        // Act & Assert
        Should.Throw<ValidationException>(async () =>
            await _handler.HandleAsync(command, CancellationToken.None));

        _eventHubMock.Verify(
            e => e.PublishAsync(It.IsAny<ContainerUpdatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public void HandleAsync_WithNonExistentContainer_ShouldNotCallCache()
    {
        // Arrange
        SetupContainersDbSet(new List<Container>());

        var command = new UpdateContainerCommand
        {
            ContainerId = 999,
            Name = _faker.Commerce.ProductName(),
            Description = _faker.Lorem.Sentence()
        };

        // Act & Assert
        Should.Throw<ValidationException>(async () =>
            await _handler.HandleAsync(command, CancellationToken.None));

        _cacheMock.Verify(
            c => c.SetAsync(It.IsAny<string>(), It.IsAny<Container>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public void HandleAsync_WithNonExistentContainer_ShouldNotPublishEvent()
    {
        // Arrange
        SetupContainersDbSet(new List<Container>());

        var command = new UpdateContainerCommand
        {
            ContainerId = 999,
            Name = _faker.Commerce.ProductName(),
            Description = _faker.Lorem.Sentence()
        };

        // Act & Assert
        Should.Throw<ValidationException>(async () =>
            await _handler.HandleAsync(command, CancellationToken.None));

        _eventHubMock.Verify(
            e => e.PublishAsync(It.IsAny<ContainerUpdatedEvent>(), It.IsAny<CancellationToken>()),
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

    private UpdateContainerCommand CreateValidCommand(int containerId)
    {
        return new UpdateContainerCommand
        {
            ContainerId = containerId,
            Name = _faker.Commerce.ProductName(),
            Description = _faker.Lorem.Sentence()
        };
    }

    private void SetupContainersDbSet(List<Container> containers)
    {
        var queryable = containers.AsQueryable();
        var mockDbSet = new Mock<DbSet<Container>>();

        mockDbSet.As<IQueryable<Container>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Container>(queryable.Provider));
        mockDbSet.As<IQueryable<Container>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockDbSet.As<IQueryable<Container>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockDbSet.As<IQueryable<Container>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
        mockDbSet.As<IAsyncEnumerable<Container>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<Container>(queryable.GetEnumerator()));

        _repositoryMock.Setup(r => r.Containers).Returns(mockDbSet.Object);
    }

    #endregion
}

#region Test Async Helpers

internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object? Execute(Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var expectedResultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethod(
                name: nameof(IQueryProvider.Execute),
                genericParameterCount: 1,
                types: new[] { typeof(Expression) })!
            .MakeGenericMethod(expectedResultType)
            .Invoke(this, new[] { expression });

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(expectedResultType)
            .Invoke(null, new[] { executionResult })!;
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    { }

    public TestAsyncEnumerable(Expression expression)
        : base(expression)
    { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public ValueTask<bool> MoveNextAsync()
    {
        return ValueTask.FromResult(_inner.MoveNext());
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }
}

#endregion
