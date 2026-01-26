using System.Linq.Expressions;
using Application.DTOs;
using Application.Features.WorkOrders.GetAllWorkOrders;
using Application.Infrastructure;
using Bogus;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace UnitTests.Features.WorkOrders.GetAllWorkOrders;

[TestFixture]
public class GetAllWorkOrdersQueryHandlerTests
{
    private Mock<IRepository> _repositoryMock = null!;
    private GetAllWorkOrdersQueryHandler _handler = null!;
    private Faker _faker = null!;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<IRepository>();
        _faker = new Faker();

        _handler = new GetAllWorkOrdersQueryHandler(_repositoryMock.Object);
    }

    #region Expected Return Values

    [Test]
    public async Task HandleAsync_WithWorkOrders_ShouldReturnWorkOrderDtos()
    {
        // Arrange
        var workOrders = new List<WorkOrder>
        {
            new() { WorkOrderId = Guid.NewGuid(), Title = _faker.Lorem.Sentence() },
            new() { WorkOrderId = Guid.NewGuid(), Title = _faker.Lorem.Sentence() }
        };
        SetupWorkOrdersDbSet(workOrders);

        // Act
        var result = await _handler.HandleAsync(new GetAllWorkOrdersQuery(), CancellationToken.None);

        // Assert
        var resultList = result.ToList();
        resultList.Count.ShouldBe(2);
        resultList.All(r => r is WorkOrderDto).ShouldBeTrue();
    }

    [Test]
    public async Task HandleAsync_WithNoWorkOrders_ShouldReturnEmptyCollection()
    {
        // Arrange
        SetupWorkOrdersDbSet(new List<WorkOrder>());

        // Act
        var result = await _handler.HandleAsync(new GetAllWorkOrdersQuery(), CancellationToken.None);

        // Assert
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task HandleAsync_WithWorkOrders_ShouldMapCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = _faker.Lorem.Sentence();
        var workOrders = new List<WorkOrder>
        {
            new() { WorkOrderId = id, Title = title }
        };
        SetupWorkOrdersDbSet(workOrders);

        // Act
        var result = await _handler.HandleAsync(new GetAllWorkOrdersQuery(), CancellationToken.None);

        // Assert
        var resultList = result.ToList();
        resultList.Count.ShouldBe(1);
        resultList[0].WorkOrderId.ShouldBe(id);
        resultList[0].Title.ShouldBe(title);
    }

    #endregion

    #region Helper Methods

    private void SetupWorkOrdersDbSet(List<WorkOrder> workOrders)
    {
        var queryable = workOrders.AsQueryable();
        var mockDbSet = new Mock<DbSet<WorkOrder>>();

        mockDbSet.As<IQueryable<WorkOrder>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<WorkOrder>(queryable.Provider));
        mockDbSet.As<IQueryable<WorkOrder>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockDbSet.As<IQueryable<WorkOrder>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockDbSet.As<IQueryable<WorkOrder>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
        mockDbSet.As<IAsyncEnumerable<WorkOrder>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<WorkOrder>(queryable.GetEnumerator()));

        _repositoryMock.Setup(r => r.WorkOrders).Returns(mockDbSet.Object);
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
