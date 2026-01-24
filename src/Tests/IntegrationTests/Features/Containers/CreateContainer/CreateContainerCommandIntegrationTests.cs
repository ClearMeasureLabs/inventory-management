using Application.Features.Containers.CreateContainer;
using Bogus;
using IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Features.Containers.CreateContainer;

[TestFixture]
public class CreateContainerCommandIntegrationTests
{
    private TestEnvironment _testEnvironment = null!;
    private IServiceProvider _serviceProvider = null!;
    private Faker _faker = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _testEnvironment = new TestEnvironment();
        await _testEnvironment.InitializeAsync();

        var builder = new ServiceProviderBuilder(_testEnvironment);
        _serviceProvider = await builder.BuildAsync();

        _faker = new Faker();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_serviceProvider is IDisposable disposable)
            disposable.Dispose();

        await _testEnvironment.DisposeAsync();
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

    [Test]
    public async Task HandleAsync_WithValidNameOnly_ShouldSucceedWithoutErrors()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICreateContainerCommandHandler>();

        var command = new CreateContainerCommand
        {
            Name = _faker.Commerce.ProductName()
        };

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ContainerId.ShouldBeGreaterThan(0);
        result.Name.ShouldBe(command.Name);
        result.Description.ShouldBe(string.Empty);
    }
}
