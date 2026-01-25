using Application.Features.Containers;
using Application.Features.Containers.CreateContainer;
using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Contracts;
using WebAPI.Controllers;
using ValidationException = Application.Exceptions.ValidationException;

namespace UnitTests.Controllers;

[TestFixture]
public class ContainersControllerTests
{
    private Mock<IContainers> _containersMock = null!;
    private ContainersController _controller = null!;
    private Faker _faker = null!;

    [SetUp]
    public void SetUp()
    {
        _containersMock = new Mock<IContainers>();
        _controller = new ContainersController(_containersMock.Object);
        _faker = new Faker();
    }

    #region CreateContainer Success Tests

    [Test]
    public async Task CreateContainer_WithValidRequest_ShouldReturn201Created()
    {
        // Arrange
        var request = new CreateContainerRequest
        {
            Name = _faker.Commerce.ProductName()
        };
        var expectedDto = new ContainerDto
        {
            ContainerId = _faker.Random.Int(1, 1000),
            Name = request.Name
        };

        _containersMock
            .Setup(c => c.CreateAsync(It.IsAny<CreateContainerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.CreateContainer(request, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<CreatedAtActionResult>();
        var createdResult = (CreatedAtActionResult)result;
        createdResult.StatusCode.ShouldBe(StatusCodes.Status201Created);
    }

    [Test]
    public async Task CreateContainer_WithValidRequest_ShouldReturnContainerResponse()
    {
        // Arrange
        var request = new CreateContainerRequest
        {
            Name = _faker.Commerce.ProductName(),
            Description = _faker.Lorem.Sentence()
        };
        var expectedDto = new ContainerDto
        {
            ContainerId = _faker.Random.Int(1, 1000),
            Name = request.Name,
            Description = request.Description
        };

        _containersMock
            .Setup(c => c.CreateAsync(It.IsAny<CreateContainerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.CreateContainer(request, CancellationToken.None);

        // Assert
        var createdResult = (CreatedAtActionResult)result;
        var response = createdResult.Value.ShouldBeOfType<ContainerResponse>();
        response.ContainerId.ShouldBe(expectedDto.ContainerId);
        response.Name.ShouldBe(expectedDto.Name);
        response.Description.ShouldBe(expectedDto.Description);
    }

    [Test]
    public async Task CreateContainer_WithValidRequest_ShouldCallContainersFacadeWithMappedCommand()
    {
        // Arrange
        var request = new CreateContainerRequest
        {
            Name = _faker.Commerce.ProductName(),
            Description = _faker.Lorem.Sentence()
        };
        var expectedDto = new ContainerDto
        {
            ContainerId = _faker.Random.Int(1, 1000),
            Name = request.Name
        };

        _containersMock
            .Setup(c => c.CreateAsync(It.IsAny<CreateContainerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        await _controller.CreateContainer(request, CancellationToken.None);

        // Assert - Verify request is correctly mapped to command
        _containersMock.Verify(
            c => c.CreateAsync(
                It.Is<CreateContainerCommand>(cmd => 
                    cmd.Name == request.Name && 
                    cmd.Description == request.Description),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region CreateContainer Validation Error Tests

    [Test]
    public async Task CreateContainer_WithInvalidRequest_ShouldReturnValidationProblem()
    {
        // Arrange
        var request = new CreateContainerRequest
        {
            Name = string.Empty
        };

        _containersMock
            .Setup(c => c.CreateAsync(It.IsAny<CreateContainerCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException
            {
                Errors = new Dictionary<string, string[]>
                {
                    { "Name", new[] { "Name is required" } }
                }
            });

        // Act
        var result = await _controller.CreateContainer(request, CancellationToken.None);

        // Assert - ValidationProblem() returns ObjectResult containing ValidationProblemDetails
        result.ShouldBeAssignableTo<ObjectResult>();
        var objectResult = (ObjectResult)result;
        objectResult.Value.ShouldBeOfType<ValidationProblemDetails>();
        var problemDetails = (ValidationProblemDetails)objectResult.Value!;
        problemDetails.Errors.ShouldContainKey("Name");
    }

    [Test]
    public async Task CreateContainer_WithInvalidRequest_ShouldReturnValidationProblemDetails()
    {
        // Arrange
        var request = new CreateContainerRequest
        {
            Name = string.Empty
        };

        _containersMock
            .Setup(c => c.CreateAsync(It.IsAny<CreateContainerCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException
            {
                Errors = new Dictionary<string, string[]>
                {
                    { "Name", new[] { "Name is required" } }
                }
            });

        // Act
        var result = await _controller.CreateContainer(request, CancellationToken.None);

        // Assert
        var objectResult = (ObjectResult)result;
        objectResult.Value.ShouldBeOfType<ValidationProblemDetails>();
        var problemDetails = (ValidationProblemDetails)objectResult.Value!;
        problemDetails.Errors.ShouldContainKey("Name");
        problemDetails.Errors["Name"].ShouldContain("Name is required");
    }

    [Test]
    public async Task CreateContainer_WithMultipleValidationErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var request = new CreateContainerRequest
        {
            Name = string.Empty
        };

        _containersMock
            .Setup(c => c.CreateAsync(It.IsAny<CreateContainerCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException
            {
                Errors = new Dictionary<string, string[]>
                {
                    { "Name", new[] { "Name is required", "Name cannot be empty" } }
                }
            });

        // Act
        var result = await _controller.CreateContainer(request, CancellationToken.None);

        // Assert
        var objectResult = (ObjectResult)result;
        var problemDetails = (ValidationProblemDetails)objectResult.Value!;
        problemDetails.Errors["Name"].Length.ShouldBe(2);
    }

    #endregion

    #region GetAllContainers Tests

    [Test]
    public async Task GetAllContainers_ShouldReturnOkResult()
    {
        // Arrange
        var containers = new List<ContainerDto>
        {
            new() { ContainerId = 1, Name = _faker.Commerce.ProductName() },
            new() { ContainerId = 2, Name = _faker.Commerce.ProductName() }
        };

        _containersMock
            .Setup(c => c.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(containers);

        // Act
        var result = await _controller.GetAllContainers(CancellationToken.None);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.StatusCode.ShouldBe(StatusCodes.Status200OK);
    }

    [Test]
    public async Task GetAllContainers_ShouldReturnMappedContainerResponses()
    {
        // Arrange
        var containers = new List<ContainerDto>
        {
            new() { ContainerId = 1, Name = "Container 1", Description = "Desc 1" },
            new() { ContainerId = 2, Name = "Container 2", Description = "Desc 2" }
        };

        _containersMock
            .Setup(c => c.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(containers);

        // Act
        var result = await _controller.GetAllContainers(CancellationToken.None);

        // Assert
        var okResult = (OkObjectResult)result;
        var responses = okResult.Value.ShouldBeAssignableTo<IEnumerable<ContainerResponse>>();
        var responseList = responses!.ToList();
        responseList.Count.ShouldBe(2);
        responseList[0].ContainerId.ShouldBe(1);
        responseList[0].Name.ShouldBe("Container 1");
        responseList[1].ContainerId.ShouldBe(2);
        responseList[1].Name.ShouldBe("Container 2");
    }

    [Test]
    public async Task GetAllContainers_ShouldCallContainersFacade()
    {
        // Arrange
        _containersMock
            .Setup(c => c.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContainerDto>());

        // Act
        await _controller.GetAllContainers(CancellationToken.None);

        // Assert
        _containersMock.Verify(c => c.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
