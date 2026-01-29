using Application.Features.Containers;
using Application.Features.Containers.GetContainerById;
using Application.Infrastructure;
using Bogus;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace UnitTests.Features.Containers.GetContainerById;

[TestFixture]
public class GetContainerByIdQueryHandlerTests
{
    private Mock<IRepository> _repositoryMock = null!;
    private GetContainerByIdQueryHandler _handler = null!;
    private Faker _faker = null!;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<IRepository>();
        _faker = new Faker();
        _handler = new GetContainerByIdQueryHandler(_repositoryMock.Object);
    }

    #region Expected Dependencies Are Called

    [Test]
    public async Task HandleAsync_ShouldAccessRepositoryContainers()
    {
        // Arrange
        var containerId = _faker.Random.Int(1, 1000);
        var containers = CreateMockDbSet(new List<Container>());
        _repositoryMock.Setup(r => r.Containers).Returns(containers.Object);
        var query = new GetContainerByIdQuery { ContainerId = containerId };

        // Act
        await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.Containers, Times.AtLeastOnce);
    }

    #endregion

    #region Expected Return Values Are Present

    [Test]
    public async Task HandleAsync_WhenContainerDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var containerId = _faker.Random.Int(1, 1000);
        var containers = CreateMockDbSet(new List<Container>());
        _repositoryMock.Setup(r => r.Containers).Returns(containers.Object);
        var query = new GetContainerByIdQuery { ContainerId = containerId };

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task HandleAsync_WhenContainerExists_ShouldReturnContainerDto()
    {
        // Arrange
        var container = new Container
        {
            ContainerId = _faker.Random.Int(1, 1000),
            Name = _faker.Commerce.ProductName(),
            Description = _faker.Lorem.Sentence()
        };
        var containers = CreateMockDbSet(new List<Container> { container });
        _repositoryMock.Setup(r => r.Containers).Returns(containers.Object);
        var query = new GetContainerByIdQuery { ContainerId = container.ContainerId };

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<ContainerDto>();
    }

    #endregion

    #region Expected Business Logic Executed

    [Test]
    public async Task HandleAsync_WhenContainerExists_ShouldMapAllProperties()
    {
        // Arrange
        var container = new Container
        {
            ContainerId = _faker.Random.Int(1, 1000),
            Name = _faker.Commerce.ProductName(),
            Description = _faker.Lorem.Sentence()
        };
        var containers = CreateMockDbSet(new List<Container> { container });
        _repositoryMock.Setup(r => r.Containers).Returns(containers.Object);
        var query = new GetContainerByIdQuery { ContainerId = container.ContainerId };

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result!.ContainerId.ShouldBe(container.ContainerId);
        result.Name.ShouldBe(container.Name);
        result.Description.ShouldBe(container.Description);
    }

    [Test]
    public async Task HandleAsync_WhenMultipleContainersExist_ShouldReturnCorrectContainer()
    {
        // Arrange
        var targetContainer = new Container
        {
            ContainerId = 2,
            Name = _faker.Commerce.ProductName(),
            Description = _faker.Lorem.Sentence()
        };
        var containerList = new List<Container>
        {
            new Container { ContainerId = 1, Name = _faker.Commerce.ProductName(), Description = _faker.Lorem.Sentence() },
            targetContainer,
            new Container { ContainerId = 3, Name = _faker.Commerce.ProductName(), Description = _faker.Lorem.Sentence() }
        };
        var containers = CreateMockDbSet(containerList);
        _repositoryMock.Setup(r => r.Containers).Returns(containers.Object);
        var query = new GetContainerByIdQuery { ContainerId = targetContainer.ContainerId };

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result!.ContainerId.ShouldBe(targetContainer.ContainerId);
        result.Name.ShouldBe(targetContainer.Name);
    }

    #endregion

    #region Helper Methods

    private static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var mockSet = new Mock<DbSet<T>>();

        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));

        mockSet.As<IQueryable<T>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));

        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

        return mockSet;
    }

    #endregion
}

#region Async Query Helpers

internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object? Execute(System.Linq.Expressions.Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default)
    {
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethod(
                name: nameof(IQueryProvider.Execute),
                genericParameterCount: 1,
                types: new[] { typeof(System.Linq.Expressions.Expression) })!
            .MakeGenericMethod(resultType)
            .Invoke(this, new[] { expression });

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(resultType)
            .Invoke(null, new[] { executionResult })!;
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    { }

    public TestAsyncEnumerable(System.Linq.Expressions.Expression expression)
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
