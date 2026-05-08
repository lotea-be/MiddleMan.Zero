using IceCreamTruck.Contracts;
using IceCreamTruck.Repositories;

using Microsoft.Extensions.DependencyInjection;

using MiddleMan.Zero;
using MiddleMan.Zero.Abstractions;

namespace IceCreamTruck.Tests.Handlers;

/// <summary>
/// Tests demonstrating MiddleMan.Zero features with GetOrderHandler.
/// These tests focus on:
/// - Handler validation via ValidateAsync
/// - Context-based message logging
/// - ResultStatus handling (Success, Invalid, NotFound)
/// </summary>
public class GetOrderHandlerTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IOrderRepository _repository;

    public GetOrderHandlerTests()
    {
        var services = new ServiceCollection();

        // Register repository
        services.AddIceCreamTruck();
        services.AddMiddleManZero();
        _serviceProvider = services.BuildServiceProvider();
        _repository = _serviceProvider.GetRequiredService<IOrderRepository>();
    }

    [Fact]
    public async Task HandleAsync_ReturnsSuccessfulResult_WhenOrderExists()
    {
        // Arrange
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = "John Doe",
            Items = [new IceCream { Flavor = "Vanilla", Scoops = 2, Price = 5.00m }],
            Status = OrderStatus.Pending
        };
        await _repository.AddAsync(order, TestContext.Current.CancellationToken);

        var handler = _serviceProvider.GetRequiredService<IHandleAsync<GetOrderRequest, Order?>>();
        var request = new GetOrderRequest { OrderId = order.Id };

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Successful),
            () => result.Response.ShouldNotBeNull(),
            () => result.Response!.Id.ShouldBe(order.Id),
            () => result.Response!.CustomerName.ShouldBe("John Doe")
        );
    }

    [Fact]
    public async Task HandleAsync_ReturnsNotFoundResult_WhenOrderDoesNotExist()
    {
        // Arrange
        var handler = _serviceProvider.GetRequiredService<IHandleAsync<GetOrderRequest, Order?>>();
        var request = new GetOrderRequest { OrderId = Guid.NewGuid() };

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.NotFound),
            () => result.Response.ShouldBeNull(),
            () => result.Messages.OfType<NotFoundMessage>().ShouldNotBeEmpty()
        );
    }

    [Fact]
    public async Task HandleAsync_ReturnsInvalidResult_WhenOrderIdIsEmpty()
    {
        // Arrange
        var handler = _serviceProvider.GetRequiredService<IHandleAsync<GetOrderRequest, Order?>>();
        var request = new GetOrderRequest { OrderId = Guid.Empty };

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        var invalidMessage = result.Messages.OfType<InvalidRequestMessage>().First();

        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Invalid),
            () => result.Response.ShouldBeNull(),
            () => result.Messages.OfType<InvalidRequestMessage>().ShouldNotBeEmpty(),
            () => invalidMessage.Message.ShouldContain("OrderId must be a valid non-empty GUID")
        );
    }

    [Fact]
    public async Task HandleAsync_DoesNotExecuteHandler_WhenValidationFails()
    {
        // Arrange
        var handler = _serviceProvider.GetRequiredService<IHandleAsync<GetOrderRequest, Order?>>();
        var request = new GetOrderRequest { OrderId = Guid.Empty };

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert - Demonstrates fail-fast pattern: validation fails, handler never executes
        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Invalid),
            () => result.Response.ShouldBeNull()
        );
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}