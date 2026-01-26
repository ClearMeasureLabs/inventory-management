using Application.Features.WorkOrders.CreateWorkOrder;
using Bogus;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Features.WorkOrders.CreateWorkOrder;

[TestFixture]
public class CreateWorkOrderCommandIntegrationTests
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
    public async Task HandleAsync_WithValidCommand_ShouldSucceedWithoutErrors()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICreateWorkOrderCommandHandler>();

        var command = new CreateWorkOrderCommand
        {
            Title = _faker.Lorem.Sentence()
        };

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.WorkOrderId.ShouldNotBe(Guid.Empty);
        result.Title.ShouldBe(command.Title);
    }

    [Test]
    public async Task HandleAsync_WithEmptyTitle_ShouldSucceedWithoutErrors()
    {
        // Arrange - No validation on Title
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICreateWorkOrderCommandHandler>();

        var command = new CreateWorkOrderCommand
        {
            Title = string.Empty
        };

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.WorkOrderId.ShouldNotBe(Guid.Empty);
        result.Title.ShouldBe(string.Empty);
    }
}
