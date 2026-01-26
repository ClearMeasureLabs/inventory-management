using Application.Features.WorkOrders.CreateWorkOrder;
using Application.Features.WorkOrders.GetAllWorkOrders;
using Bogus;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Features.WorkOrders.GetAllWorkOrders;

[TestFixture]
public class GetAllWorkOrdersQueryIntegrationTests
{
    private IServiceProvider _serviceProvider = null!;
    private Faker _faker = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Use shared service provider from global fixture
        _serviceProvider = GlobalTestFixture.ServiceProvider;
        _faker = new Faker();
    }

    [Test]
    public async Task HandleAsync_ShouldReturnWorkOrders()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var createHandler = scope.ServiceProvider.GetRequiredService<ICreateWorkOrderCommandHandler>();
        var queryHandler = scope.ServiceProvider.GetRequiredService<IGetAllWorkOrdersQueryHandler>();

        // Create a work order first
        var createCommand = new CreateWorkOrderCommand
        {
            Title = _faker.Lorem.Sentence()
        };
        await createHandler.HandleAsync(createCommand, CancellationToken.None);

        // Act
        var result = await queryHandler.HandleAsync(new GetAllWorkOrdersQuery(), CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
    }

    [Test]
    public async Task HandleAsync_ShouldReturnCreatedWorkOrder()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var createHandler = scope.ServiceProvider.GetRequiredService<ICreateWorkOrderCommandHandler>();
        var queryHandler = scope.ServiceProvider.GetRequiredService<IGetAllWorkOrdersQueryHandler>();

        var uniqueTitle = $"Test Work Order {Guid.NewGuid()}";
        var createCommand = new CreateWorkOrderCommand
        {
            Title = uniqueTitle
        };
        var createdWorkOrder = await createHandler.HandleAsync(createCommand, CancellationToken.None);

        // Act
        var result = await queryHandler.HandleAsync(new GetAllWorkOrdersQuery(), CancellationToken.None);

        // Assert
        var resultList = result.ToList();
        resultList.ShouldContain(w => w.WorkOrderId == createdWorkOrder.WorkOrderId && w.Title == uniqueTitle);
    }
}
