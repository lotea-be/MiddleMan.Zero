using IceCreamTruck.Contracts;

using Microsoft.Extensions.DependencyInjection;

using MiddleMan.Zero;
using MiddleMan.Zero.Abstractions;

namespace IceCreamTruck.Tests.Handlers;

/// <summary>
/// Tests demonstrating MiddleMan.Zero features with CancelOrderHandler.
/// These tests focus on:
/// - Forbidden status: non-admin users cannot cancel orders
/// - NotFound status: cancelling a non-existent order
/// - Invalid status: empty order ID
/// - Successful status: admin user cancels an existing order
/// </summary>
public class CancelOrderHandlerTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IOrderRepository _repository;

    public CancelOrderHandlerTests()
    {
        var services = new ServiceCollection();
        services.AddIceCreamTruck();
        _serviceProvider = services.BuildServiceProvider();
        _repository = _serviceProvider.GetRequiredService<IOrderRepository>();
    }

    [Fact]
    public async Task HandleAsync_ReturnsForbiddenResult_WhenCallerIsNotAdmin()
    {
        // Arrange
        var order = new Order
        {
            CustomerName = "John Doe",
            Items = [new IceCream { Flavor = "Vanilla", Price = 5.00m, Scoops = 1 }],
            Status = OrderStatus.Pending
        };
        await _repository.AddAsync(order, TestContext.Current.CancellationToken);

        var handler = _serviceProvider.GetRequiredService<IHandleAsync<CancelOrderRequest>>();
        var request = new CancelOrderRequest { OrderId = order.Id, IsAdminUser = false };

        // Act - Non-admin user attempts to cancel an order
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert - MiddleMan.Zero converts ForbiddenMessage to Forbidden status
        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Forbidden),
            () => result.Messages.OfType<ForbiddenMessage>().ShouldNotBeEmpty()
        );
    }

    [Fact]
    public async Task HandleAsync_DoesNotCancelOrder_WhenCallerIsNotAdmin()
    {
        // Arrange
        var order = new Order
        {
            CustomerName = "Jane Smith",
            Items = [new IceCream { Flavor = "Chocolate", Price = 6.50m, Scoops = 2 }],
            Status = OrderStatus.Pending
        };
        await _repository.AddAsync(order, TestContext.Current.CancellationToken);

        var handler = _serviceProvider.GetRequiredService<IHandleAsync<CancelOrderRequest>>();
        var request = new CancelOrderRequest { OrderId = order.Id, IsAdminUser = false };

        // Act
        await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert - Order remains unchanged
        var unchanged = await _repository.GetAsync(order.Id, TestContext.Current.CancellationToken);
        unchanged!.Status.ShouldBe(OrderStatus.Pending);
    }

    [Fact]
    public async Task HandleAsync_ReturnsSuccessfulResult_WhenAdminCancelsExistingOrder()
    {
        // Arrange
        var order = new Order
        {
            CustomerName = "Alice",
            Items = [new IceCream { Flavor = "Strawberry", Price = 4.50m, Scoops = 1 }],
            Status = OrderStatus.Pending
        };
        await _repository.AddAsync(order, TestContext.Current.CancellationToken);

        var handler = _serviceProvider.GetRequiredService<IHandleAsync<CancelOrderRequest>>();
        var request = new CancelOrderRequest { OrderId = order.Id, IsAdminUser = true };

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Successful)
        );

        var cancelled = await _repository.GetAsync(order.Id, TestContext.Current.CancellationToken);
        cancelled!.Status.ShouldBe(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task HandleAsync_ReturnsNotFoundResult_WhenAdminCancelsNonExistentOrder()
    {
        // Arrange
        var handler = _serviceProvider.GetRequiredService<IHandleAsync<CancelOrderRequest>>();
        var request = new CancelOrderRequest { OrderId = Guid.NewGuid(), IsAdminUser = true };

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.NotFound),
            () => result.Messages.OfType<NotFoundMessage>().ShouldNotBeEmpty()
        );
    }

    [Fact]
    public async Task HandleAsync_ReturnsInvalidResult_WhenOrderIdIsEmpty()
    {
        // Arrange
        var handler = _serviceProvider.GetRequiredService<IHandleAsync<CancelOrderRequest>>();
        var request = new CancelOrderRequest { OrderId = Guid.Empty, IsAdminUser = true };

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert - Validation runs before the Forbidden/NotFound checks
        var invalidMessage = result.Messages.OfType<InvalidRequestMessage>().First();

        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Invalid),
            () => invalidMessage.Message.ShouldContain("OrderId must be a valid non-empty GUID")
        );
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
