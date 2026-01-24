using Application.Features.Containers;
using Application.Features.Containers.GetAllContainers;
using Application.Infrastructure;
using Bogus;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MockQueryable;

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
        var containers = new List<Container>().AsQueryable().BuildMockDbSet();
        _repositoryMock.Setup(r => r.Containers).Returns(containers.Object);
        var query = new GetAllContainersQuery();

        // Act
        await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.Containers, Times.Once);
    }

    #endregion

    #region Expected Return Values Are Present

    [Test]
    public async Task HandleAsync_WhenNoContainersExist_ShouldReturnEmptyCollection()
    {
        // Arrange
        var containers = new List<Container>().AsQueryable().BuildMockDbSet();
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
        var containers = containerList.AsQueryable().BuildMockDbSet();
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
        var containers = containerList.AsQueryable().BuildMockDbSet();
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
        var containers = containerList.AsQueryable().BuildMockDbSet();
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
        var containers = containerList.AsQueryable().BuildMockDbSet();
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
        var containers = new List<Container> { container }.AsQueryable().BuildMockDbSet();
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

    #endregion
}
