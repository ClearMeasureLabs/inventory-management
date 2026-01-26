using Application.DTOs;
using Application.Features.WorkOrders;
using Application.Features.WorkOrders.CreateWorkOrder;
using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Contracts;
using WebAPI.Controllers;
using ValidationException = Application.Exceptions.ValidationException;

namespace UnitTests.Controllers;

[TestFixture]
public class WorkOrdersControllerTests
{
    private Mock<IWorkOrders> _workOrdersMock = null!;
    private WorkOrdersController _controller = null!;
    private Faker _faker = null!;

    [SetUp]
    public void SetUp()
    {
        _workOrdersMock = new Mock<IWorkOrders>();
        _controller = new WorkOrdersController(_workOrdersMock.Object);
        _faker = new Faker();
    }

    #region CreateWorkOrder Success Tests

    [Test]
    public async Task CreateWorkOrder_WithValidRequest_ShouldReturn201Created()
    {
        // Arrange
        var request = new CreateWorkOrderRequest
        {
            Title = _faker.Lorem.Sentence()
        };
        var expectedDto = new WorkOrderDto
        {
            WorkOrderId = Guid.NewGuid(),
            Title = request.Title
        };

        _workOrdersMock
            .Setup(w => w.CreateAsync(It.IsAny<CreateWorkOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.CreateWorkOrder(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<CreatedAtActionResult>();
        var createdResult = (CreatedAtActionResult)result;
        createdResult.StatusCode.ShouldBe(StatusCodes.Status201Created);
    }

    [Test]
    public async Task CreateWorkOrder_WithValidRequest_ShouldReturnWorkOrderResponse()
    {
        // Arrange
        var request = new CreateWorkOrderRequest
        {
            Title = _faker.Lorem.Sentence()
        };
        var expectedDto = new WorkOrderDto
        {
            WorkOrderId = Guid.NewGuid(),
            Title = request.Title
        };

        _workOrdersMock
            .Setup(w => w.CreateAsync(It.IsAny<CreateWorkOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.CreateWorkOrder(request, CancellationToken.None);

        // Assert
        var createdResult = (CreatedAtActionResult)result;
        var response = createdResult.Value.ShouldBeOfType<WorkOrderResponse>();
        response.WorkOrderId.ShouldBe(expectedDto.WorkOrderId);
        response.Title.ShouldBe(expectedDto.Title);
    }

    [Test]
    public async Task CreateWorkOrder_WithValidRequest_ShouldCallWorkOrdersFacadeWithMappedCommand()
    {
        // Arrange
        var request = new CreateWorkOrderRequest
        {
            Title = _faker.Lorem.Sentence()
        };
        var expectedDto = new WorkOrderDto
        {
            WorkOrderId = Guid.NewGuid(),
            Title = request.Title
        };

        _workOrdersMock
            .Setup(w => w.CreateAsync(It.IsAny<CreateWorkOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        await _controller.CreateWorkOrder(request, CancellationToken.None);

        // Assert - Verify request is correctly mapped to command
        _workOrdersMock.Verify(
            w => w.CreateAsync(
                It.Is<CreateWorkOrderCommand>(cmd => cmd.Title == request.Title),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region CreateWorkOrder Validation Error Tests

    [Test]
    public async Task CreateWorkOrder_WithInvalidRequest_ShouldReturnValidationProblem()
    {
        // Arrange
        var request = new CreateWorkOrderRequest
        {
            Title = string.Empty
        };

        _workOrdersMock
            .Setup(w => w.CreateAsync(It.IsAny<CreateWorkOrderCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException
            {
                Errors = new Dictionary<string, string[]>
                {
                    { "Title", new[] { "Title is required" } }
                }
            });

        // Act
        var result = await _controller.CreateWorkOrder(request, CancellationToken.None);

        // Assert - ValidationProblem() returns ObjectResult containing ValidationProblemDetails
        result.ShouldBeAssignableTo<ObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.Value.ShouldBeOfType<ValidationProblemDetails>();
        var problemDetails = (ValidationProblemDetails)objectResult.Value!;
        problemDetails.Errors.ShouldContainKey("Title");
    }

    #endregion

    #region GetAllWorkOrders Tests

    [Test]
    public async Task GetAllWorkOrders_ShouldReturnOkResult()
    {
        // Arrange
        var workOrders = new List<WorkOrderDto>
        {
            new() { WorkOrderId = Guid.NewGuid(), Title = _faker.Lorem.Sentence() },
            new() { WorkOrderId = Guid.NewGuid(), Title = _faker.Lorem.Sentence() }
        };

        _workOrdersMock
            .Setup(w => w.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrders);

        // Act
        var result = await _controller.GetAllWorkOrders(CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.StatusCode.ShouldBe(StatusCodes.Status200OK);
    }

    [Test]
    public async Task GetAllWorkOrders_ShouldReturnMappedWorkOrderResponses()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var workOrders = new List<WorkOrderDto>
        {
            new() { WorkOrderId = id1, Title = "Work Order 1" },
            new() { WorkOrderId = id2, Title = "Work Order 2" }
        };

        _workOrdersMock
            .Setup(w => w.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(workOrders);

        // Act
        var result = await _controller.GetAllWorkOrders(CancellationToken.None);

        // Assert
        var okResult = (OkObjectResult)result;
        var responses = okResult.Value.ShouldBeAssignableTo<IEnumerable<WorkOrderResponse>>();
        var responseList = responses!.ToList();
        responseList.Count.ShouldBe(2);
        responseList[0].WorkOrderId.ShouldBe(id1);
        responseList[0].Title.ShouldBe("Work Order 1");
        responseList[1].WorkOrderId.ShouldBe(id2);
        responseList[1].Title.ShouldBe("Work Order 2");
    }

    [Test]
    public async Task GetAllWorkOrders_ShouldCallWorkOrdersFacade()
    {
        // Arrange
        _workOrdersMock
            .Setup(w => w.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WorkOrderDto>());

        // Act
        await _controller.GetAllWorkOrders(CancellationToken.None);

        // Assert
        _workOrdersMock.Verify(w => w.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region DeleteWorkOrder Tests

    [Test]
    public async Task DeleteWorkOrder_WithValidId_ShouldReturn204NoContent()
    {
        // Arrange
        var workOrderId = Guid.NewGuid();

        _workOrdersMock
            .Setup(w => w.DeleteAsync(It.IsAny<Application.Features.WorkOrders.DeleteWorkOrder.DeleteWorkOrderCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteWorkOrder(workOrderId, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<NoContentResult>();
    }

    [Test]
    public async Task DeleteWorkOrder_WithNonExistentId_ShouldReturnValidationProblem()
    {
        // Arrange
        var workOrderId = Guid.NewGuid();

        _workOrdersMock
            .Setup(w => w.DeleteAsync(It.IsAny<Application.Features.WorkOrders.DeleteWorkOrder.DeleteWorkOrderCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException
            {
                Errors = new Dictionary<string, string[]>
                {
                    { "WorkOrderId", new[] { "Work order not found" } }
                }
            });

        // Act
        var result = await _controller.DeleteWorkOrder(workOrderId, CancellationToken.None);

        // Assert
        result.ShouldBeAssignableTo<ObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.Value.ShouldBeOfType<ValidationProblemDetails>();
    }

    #endregion
}
