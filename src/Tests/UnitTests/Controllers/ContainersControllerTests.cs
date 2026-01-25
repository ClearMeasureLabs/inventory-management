using Application.Features.Containers;
using Application.Features.Containers.CreateContainer;
using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApp.Controllers;
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
        var command = new CreateContainerCommand
        {
            Name = _faker.Commerce.ProductName()
        };
        var expectedDto = new ContainerDto
        {
            ContainerId = _faker.Random.Int(1, 1000),
            Name = command.Name
        };

        _containersMock
            .Setup(c => c.CreateAsync(It.IsAny<CreateContainerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.CreateContainer(command, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<CreatedAtActionResult>();
        var createdResult = (CreatedAtActionResult)result;
        createdResult.StatusCode.ShouldBe(StatusCodes.Status201Created);
    }

    [Test]
    public async Task CreateContainer_WithValidRequest_ShouldReturnContainerDto()
    {
        // Arrange
        var command = new CreateContainerCommand
        {
            Name = _faker.Commerce.ProductName()
        };
        var expectedDto = new ContainerDto
        {
            ContainerId = _faker.Random.Int(1, 1000),
            Name = command.Name,
            Description = _faker.Lorem.Sentence()
        };

        _containersMock
            .Setup(c => c.CreateAsync(It.IsAny<CreateContainerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _controller.CreateContainer(command, CancellationToken.None);

        // Assert
        var createdResult = (CreatedAtActionResult)result;
        var returnedDto = createdResult.Value.ShouldBeOfType<ContainerDto>();
        returnedDto.ContainerId.ShouldBe(expectedDto.ContainerId);
        returnedDto.Name.ShouldBe(expectedDto.Name);
        returnedDto.Description.ShouldBe(expectedDto.Description);
    }

    [Test]
    public async Task CreateContainer_WithValidRequest_ShouldCallContainersFacade()
    {
        // Arrange
        var command = new CreateContainerCommand
        {
            Name = _faker.Commerce.ProductName()
        };
        var expectedDto = new ContainerDto
        {
            ContainerId = _faker.Random.Int(1, 1000),
            Name = command.Name
        };

        _containersMock
            .Setup(c => c.CreateAsync(It.IsAny<CreateContainerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        await _controller.CreateContainer(command, CancellationToken.None);

        // Assert
        _containersMock.Verify(
            c => c.CreateAsync(
                It.Is<CreateContainerCommand>(cmd => cmd.Name == command.Name),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region CreateContainer Validation Error Tests

    [Test]
    public async Task CreateContainer_WithInvalidRequest_ShouldReturnValidationProblem()
    {
        // Arrange
        var command = new CreateContainerCommand
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
        var result = await _controller.CreateContainer(command, CancellationToken.None);

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
        var command = new CreateContainerCommand
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
        var result = await _controller.CreateContainer(command, CancellationToken.None);

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
        var command = new CreateContainerCommand
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
        var result = await _controller.CreateContainer(command, CancellationToken.None);

        // Assert
        var objectResult = (ObjectResult)result;
        var problemDetails = (ValidationProblemDetails)objectResult.Value!;
        problemDetails.Errors["Name"].Length.ShouldBe(2);
    }

    #endregion
}
