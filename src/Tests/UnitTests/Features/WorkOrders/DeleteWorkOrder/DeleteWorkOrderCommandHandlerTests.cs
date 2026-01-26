using System.Linq.Expressions;
using Application.Features.WorkOrders.DeleteWorkOrder;
using Application.Infrastructure;
using Bogus;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using ValidationException = Application.Exceptions.ValidationException;

namespace UnitTests.Features.WorkOrders.DeleteWorkOrder;

[TestFixture]
public class DeleteWorkOrderCommandHandlerTests
{
    private Mock<IRepository> _repositoryMock = null!;
    private Mock<ICache> _cacheMock = null!;
    private Mock<IEventHub> _eventHubMock = null!;
    private DeleteWorkOrderCommandHandler _handler = null!;
    private Faker _faker = null!;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<IRepository>();
        _cacheMock = new Mock<ICache>();
        _eventHubMock = new Mock<IEventHub>();
        _faker = new Faker();

        _handler = new DeleteWorkOrderCommandHandler(
            _repositoryMock.Object,
            _cacheMock.Object,
            _eventHubMock.Object);
    }

    #region Expected Dependencies Are Called

    [Test]
    public async Task HandleAsync_WithExistingWorkOrder_ShouldRemoveFromRepository()
    {
        // Arrange
        var workOrder = CreateValidWorkOrder();
        SetupWorkOrdersDbSet(new List<WorkOrder> { workOrder });

        var command = new DeleteWorkOrderCommand { WorkOrderId = workOrder.WorkOrderId };

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            r => r.WorkOrders.Remove(It.Is<WorkOrder>(w => w.WorkOrderId == workOrder.WorkOrderId)),
            Times.Once);
    }

    [Test]
    public async Task HandleAsync_WithExistingWorkOrder_ShouldSaveChanges()
    {
        // Arrange
        var workOrder = CreateValidWorkOrder();
        SetupWorkOrdersDbSet(new List<WorkOrder> { workOrder });

        var command = new DeleteWorkOrderCommand { WorkOrderId = workOrder.WorkOrderId };

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task HandleAsync_WithExistingWorkOrder_ShouldRemoveFromCache()
    {
        // Arrange
        var workOrder = CreateValidWorkOrder();
        SetupWorkOrdersDbSet(new List<WorkOrder> { workOrder });

        var command = new DeleteWorkOrderCommand { WorkOrderId = workOrder.WorkOrderId };

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _cacheMock.Verify(
            c => c.RemoveWorkOrderAsync(It.Is<string>(key => key.Contains(workOrder.WorkOrderId.ToString())), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task HandleAsync_WithExistingWorkOrder_ShouldPublishDeletedEvent()
    {
        // Arrange
        var workOrder = CreateValidWorkOrder();
        SetupWorkOrdersDbSet(new List<WorkOrder> { workOrder });

        var command = new DeleteWorkOrderCommand { WorkOrderId = workOrder.WorkOrderId };

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _eventHubMock.Verify(
            e => e.PublishAsync(It.Is<WorkOrderDeletedEvent>(evt => evt.WorkOrderId == workOrder.WorkOrderId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Expected Exceptions Are Thrown

    [Test]
    public void HandleAsync_WithNonExistentWorkOrder_ShouldThrowValidationException()
    {
        // Arrange
        SetupWorkOrdersDbSet(new List<WorkOrder>());

        var command = new DeleteWorkOrderCommand { WorkOrderId = Guid.NewGuid() };

        // Act & Assert
        var exception = Should.Throw<ValidationException>(async () =>
            await _handler.HandleAsync(command, CancellationToken.None));

        exception.Errors.ShouldContainKey("WorkOrderId");
        exception.Errors["WorkOrderId"].ShouldContain("Work order not found");
    }

    [Test]
    public void HandleAsync_WithNonExistentWorkOrder_ShouldNotRemoveFromRepository()
    {
        // Arrange
        SetupWorkOrdersDbSet(new List<WorkOrder>());

        var command = new DeleteWorkOrderCommand { WorkOrderId = Guid.NewGuid() };

        // Act & Assert
        Should.Throw<ValidationException>(async () =>
            await _handler.HandleAsync(command, CancellationToken.None));

        _repositoryMock.Verify(
            r => r.WorkOrders.Remove(It.IsAny<WorkOrder>()),
            Times.Never);
    }

    #endregion

    #region Helper Methods

    private WorkOrder CreateValidWorkOrder()
    {
        return new WorkOrder
        {
            WorkOrderId = Guid.NewGuid(),
            Title = _faker.Lorem.Sentence()
        };
    }

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
