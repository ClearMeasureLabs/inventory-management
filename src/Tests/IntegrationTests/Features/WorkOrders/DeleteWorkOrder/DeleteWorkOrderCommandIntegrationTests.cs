using Application.Features.WorkOrders.CreateWorkOrder;
using Application.Features.WorkOrders.DeleteWorkOrder;
using Bogus;
using Microsoft.Extensions.DependencyInjection;
using ValidationException = Application.Exceptions.ValidationException;

namespace IntegrationTests.Features.WorkOrders.DeleteWorkOrder;

[TestFixture]
public class DeleteWorkOrderCommandIntegrationTests
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
    public async Task HandleAsync_WithExistingWorkOrder_ShouldSucceedWithoutErrors()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var createHandler = scope.ServiceProvider.GetRequiredService<ICreateWorkOrderCommandHandler>();
        var deleteHandler = scope.ServiceProvider.GetRequiredService<IDeleteWorkOrderCommandHandler>();

        // First create a work order
        var createCommand = new CreateWorkOrderCommand
        {
            Title = _faker.Lorem.Sentence()
        };
        var createdWorkOrder = await createHandler.HandleAsync(createCommand, CancellationToken.None);

        var deleteCommand = new DeleteWorkOrderCommand
        {
            WorkOrderId = createdWorkOrder.WorkOrderId
        };

        // Act & Assert - should not throw
        await deleteHandler.HandleAsync(deleteCommand, CancellationToken.None);
    }

    [Test]
    public void HandleAsync_WithNonExistentWorkOrder_ShouldThrowValidationException()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IDeleteWorkOrderCommandHandler>();

        var command = new DeleteWorkOrderCommand
        {
            WorkOrderId = Guid.NewGuid() // Non-existent
        };

        // Act & Assert
        var exception = Should.Throw<ValidationException>(async () =>
            await handler.HandleAsync(command, CancellationToken.None));

        exception.Errors.ShouldContainKey("WorkOrderId");
    }
}
