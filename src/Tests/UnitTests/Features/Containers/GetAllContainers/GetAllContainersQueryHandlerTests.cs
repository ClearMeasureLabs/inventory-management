using Application.Features.Containers;
using Application.Features.Containers.GetAllContainers;
using Application.Infrastructure;
using Bogus;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace UnitTests.Features.Containers.GetAllContainers;

[TestFixture]
public class GetAllContainersQueryHandlerTests
{
    private Mock<IRepository> _repositoryMock = null!;
    private GetAllContainersQueryHandler _handler = null!;
    private Faker _faker = null!;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<IRepository>();
        _faker = new Faker();
        _handler = new GetAllContainersQueryHandler(_repositoryMock.Object);
    }

    #region Expected Dependencies Are Called

    [Test]
    public async Task HandleAsync_ShouldAccessRepositoryContainers()
    {
        // Arrange
        var containers = CreateMockDbSet(new List<Container>());
        _repositoryMock.Setup(r => r.Containers).Returns(containers.Object);
        var query = new GetAllContainersQuery();

        // Act
        await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.Containers, Times.AtLeastOnce);
    }

    #endregion

    #region Expected Return Values Are Present

    [Test]
    public async Task HandleAsync_WhenNoContainersExist_ShouldReturnEmptyCollection()
    {
        // Arrange
        var containers = CreateMockDbSet(new List<Container>());
        _repositoryMock.Setup(r => r.Containers).Returns(containers.Object);
        var query = new GetAllContainersQuery();

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task HandleAsync_WhenContainersExist_ShouldReturnCorrectCount()
    {
        // Arrange
        var containerList = CreateContainers(3);
        var containers = CreateMockDbSet(containerList);
        _repositoryMock.Setup(r => r.Containers).Returns(containers.Object);
        var query = new GetAllContainersQuery();

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Count().ShouldBe(3);
    }

    [Test]
    public async Task HandleAsync_WhenContainersExist_ShouldReturnCorrectContainerIds()
    {
        // Arrange
        var containerList = CreateContainers(2);
        var containers = CreateMockDbSet(containerList);
        _repositoryMock.Setup(r => r.Containers).Returns(containers.Object);
        var query = new GetAllContainersQuery();

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        var resultList = result.ToList();
        resultList[0].ContainerId.ShouldBe(containerList[0].ContainerId);
        resultList[1].ContainerId.ShouldBe(containerList[1].ContainerId);
    }

    [Test]
    public async Task HandleAsync_WhenContainersExist_ShouldReturnCorrectContainerNames()
    {
        // Arrange
        var containerList = CreateContainers(2);
        var containers = CreateMockDbSet(containerList);
        _repositoryMock.Setup(r => r.Containers).Returns(containers.Object);
        var query = new GetAllContainersQuery();

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        var resultList = result.ToList();
        resultList[0].Name.ShouldBe(containerList[0].Name);
        resultList[1].Name.ShouldBe(containerList[1].Name);
    }

    [Test]
    public async Task HandleAsync_ShouldReturnContainerDtoType()
    {
        // Arrange
        var containerList = CreateContainers(1);
        var containers = CreateMockDbSet(containerList);
        _repositoryMock.Setup(r => r.Containers).Returns(containers.Object);
        var query = new GetAllContainersQuery();

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.First().ShouldBeOfType<ContainerDto>();
    }

    #endregion

    #region Expected Business Logic Executed

    [Test]
    public async Task HandleAsync_ShouldMapAllContainerProperties()
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
        var query = new GetAllContainersQuery();

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        var dto = result.First();
        dto.ContainerId.ShouldBe(container.ContainerId);
        dto.Name.ShouldBe(container.Name);
        dto.Description.ShouldBe(container.Description);
    }

    #endregion

    #region Helper Methods

    private List<Container> CreateContainers(int count)
    {
        var containers = new List<Container>();
        for (int i = 0; i < count; i++)
        {
            containers.Add(new Container
            {
                ContainerId = i + 1,
                Name = _faker.Commerce.ProductName(),
                Description = _faker.Lorem.Sentence()
            });
        }
        return containers;
    }

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
