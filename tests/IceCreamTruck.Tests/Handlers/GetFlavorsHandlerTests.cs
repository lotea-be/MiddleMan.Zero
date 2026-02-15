using IceCreamTruck.Contracts;

using Microsoft.Extensions.DependencyInjection;

using MiddleMan.Zero;
using MiddleMan.Zero.Abstractions;

namespace IceCreamTruck.Tests.Handlers;

/// <summary>
/// Tests demonstrating MiddleMan.Zero features with GetFlavorsHandler.
/// These tests focus on:
/// - Handler with response type (Flavor[])
/// - Validation pass-through (no validation rules)
/// - ResultStatus handling (Success)
/// </summary>
public class GetFlavorsHandlerTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public GetFlavorsHandlerTests()
    {
        var services = new ServiceCollection();

        services.AddIceCreamTruck();
        services.AddMiddleManZero();
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task HandleAsync_ReturnsSuccessfulResult_WithSeededFlavors()
    {
        // Arrange
        var handler = _serviceProvider.GetRequiredService<IHandleAsync<GetFlavorsRequest, Flavor[]>>();
        var request = new GetFlavorsRequest();

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Successful),
            () => result.Response.ShouldNotBeNull(),
            () => result.Response!.Length.ShouldBe(3),
            () => result.Response!.ShouldContain(f => f.Name == "Vanilla"),
            () => result.Response!.ShouldContain(f => f.Name == "Chocolate"),
            () => result.Response!.ShouldContain(f => f.Name == "Strawberry")
        );
    }

    [Fact]
    public async Task HandleAsync_ReturnsSuccessfulResult_WithNoMessages()
    {
        // Arrange
        var handler = _serviceProvider.GetRequiredService<IHandleAsync<GetFlavorsRequest, Flavor[]>>();
        var request = new GetFlavorsRequest();

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert - No validation errors expected
        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Successful),
            () => result.Messages.ShouldBeEmpty()
        );
    }

    [Fact]
    public async Task HandleAsync_ReturnsInvalidResult_WhenRequestIsNull()
    {
        // Arrange
        var handler = _serviceProvider.GetRequiredService<IHandleAsync<GetFlavorsRequest, Flavor[]>>();

        // Act - MiddleMan.Zero handles null request before reaching handler
        var result = await handler.HandleAsync(null!, TestContext.Current.CancellationToken);

        // Assert - Demonstrates built-in null request guard
        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Invalid),
            () => result.Response.ShouldBeNull(),
            () => result.Messages.OfType<InvalidRequestMessage>().ShouldNotBeEmpty()
        );
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
