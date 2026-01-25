using Application.Features.Containers.CreateContainer;
using Bogus;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Features.Containers.CreateContainer;

[TestFixture]
public class CreateContainerCommandIntegrationTests
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
        var handler = scope.ServiceProvider.GetRequiredService<ICreateContainerCommandHandler>();

        var command = new CreateContainerCommand
        {
            Name = _faker.Commerce.ProductName(),
            Description = _faker.Lorem.Sentence()
        };

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ContainerId.ShouldBeGreaterThan(0);
        result.Name.ShouldBe(command.Name);
        result.Description.ShouldBe(command.Description);
    }
}
