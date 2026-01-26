using Application.DTOs;
using Application.Features.WorkOrders.CreateWorkOrder;
using Application.Infrastructure;
using Bogus;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Features.WorkOrders.CreateWorkOrder;

[TestFixture]
public class CreateWorkOrderCommandHandlerTests
{
    private Mock<IRepository> _repositoryMock = null!;
    private Mock<ICache> _cacheMock = null!;
    private Mock<IEventHub> _eventHubMock = null!;
    private Mock<DbSet<WorkOrder>> _workOrdersDbSetMock = null!;
    private CreateWorkOrderCommandHandler _handler = null!;
    private Faker _faker = null!;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<IRepository>();
        _cacheMock = new Mock<ICache>();
        _eventHubMock = new Mock<IEventHub>();
        _workOrdersDbSetMock = new Mock<DbSet<WorkOrder>>();
        _faker = new Faker();

        _repositoryMock.Setup(r => r.WorkOrders).Returns(_workOrdersDbSetMock.Object);

        _handler = new CreateWorkOrderCommandHandler(
            _repositoryMock.Object,
            _cacheMock.Object,
            _eventHubMock.Object);
    }

    #region Expected Dependencies Are Called

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldAddWorkOrderToRepository()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _workOrdersDbSetMock.Verify(
            db => db.AddAsync(It.Is<WorkOrder>(w => w.Title == command.Title), 
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
    public async Task HandleAsync_WithValidCommand_ShouldCacheWorkOrder()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _cacheMock.Verify(
            c => c.SetWorkOrderAsync(
                It.Is<string>(key => key.StartsWith("WorkOrder:")),
                It.Is<WorkOrder>(workOrder => workOrder.Title == command.Title),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldPublishWorkOrderCreatedEvent()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _eventHubMock.Verify(
            e => e.PublishAsync(
                It.IsAny<WorkOrderCreatedEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldCallDependenciesInCorrectOrder()
    {
        // Arrange
        var command = CreateValidCommand();
        var callOrder = new List<string>();

        _workOrdersDbSetMock
            .Setup(db => db.AddAsync(It.IsAny<WorkOrder>(), It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("AddAsync"));

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("SaveChangesAsync"))
            .ReturnsAsync(1);

        _cacheMock
            .Setup(c => c.SetWorkOrderAsync(It.IsAny<string>(), It.IsAny<WorkOrder>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("SetWorkOrderAsync"));

        _eventHubMock
            .Setup(e => e.PublishAsync(It.IsAny<WorkOrderCreatedEvent>(), It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("PublishAsync"));

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        callOrder.ShouldBe(new[] { "AddAsync", "SaveChangesAsync", "SetWorkOrderAsync", "PublishAsync" });
    }

    #endregion

    #region Expected Business Logic Executed

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldCreateWorkOrderWithCorrectTitle()
    {
        // Arrange
        var command = CreateValidCommand();
        WorkOrder? capturedWorkOrder = null;

        _workOrdersDbSetMock
            .Setup(db => db.AddAsync(It.IsAny<WorkOrder>(), It.IsAny<CancellationToken>()))
            .Callback<WorkOrder, CancellationToken>((workOrder, _) => capturedWorkOrder = workOrder);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        capturedWorkOrder.ShouldNotBeNull();
        capturedWorkOrder.Title.ShouldBe(command.Title);
    }

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldCreateWorkOrderWithNewGuid()
    {
        // Arrange
        var command = CreateValidCommand();
        WorkOrder? capturedWorkOrder = null;

        _workOrdersDbSetMock
            .Setup(db => db.AddAsync(It.IsAny<WorkOrder>(), It.IsAny<CancellationToken>()))
            .Callback<WorkOrder, CancellationToken>((workOrder, _) => capturedWorkOrder = workOrder);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        capturedWorkOrder.ShouldNotBeNull();
        capturedWorkOrder.WorkOrderId.ShouldNotBe(Guid.Empty);
    }

    #endregion

    #region Expected Return Values Are Present

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldReturnWorkOrderDto()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<WorkOrderDto>();
    }

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldReturnDtoWithCorrectTitle()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Title.ShouldBe(command.Title);
    }

    [Test]
    public async Task HandleAsync_WithValidCommand_ShouldReturnDtoWithNonEmptyId()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.WorkOrderId.ShouldNotBe(Guid.Empty);
    }

    [Test]
    public async Task HandleAsync_WithEmptyTitle_ShouldCreateWorkOrder()
    {
        // Arrange - No validation on Title since the user said no validation
        var command = new CreateWorkOrderCommand
        {
            Title = string.Empty
        };

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Title.ShouldBe(string.Empty);
    }

    #endregion

    #region Helper Methods

    private CreateWorkOrderCommand CreateValidCommand()
    {
        return new CreateWorkOrderCommand
        {
            Title = _faker.Lorem.Sentence()
        };
    }

    #endregion
}
