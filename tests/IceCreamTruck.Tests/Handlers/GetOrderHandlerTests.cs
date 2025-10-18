using IceCreamTruck.Contracts;
using IceCreamTruck.Handlers;
using IceCreamTruck.Repositories;
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
public class GetOrderHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsSuccessfulResult_WhenOrderExists()
    {
        // Arrange
        var repository = new OrderRepository();
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = "John Doe",
            Items = [new IceCream { Flavor = "Vanilla", Price = 5.00m, Scoops = 2 }],
            Status = OrderStatus.Pending
        };
        await repository.Add(order);

        var handler = new GetOrderHandler(repository);
        var request = new GetOrderRequest { OrderId = order.Id };

        // Act - MiddleMan.Zero's HandleAsync method orchestrates validation and handling
        var result = await handler.HandleAsync(request);

        // Assert - Verify MiddleMan.Zero's Result structure
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
        var repository = new OrderRepository();
        var handler = new GetOrderHandler(repository);
        var request = new GetOrderRequest { OrderId = Guid.NewGuid() };

        // Act - Handler logs NotFoundMessage to context, MiddleMan.Zero converts to NotFound status
        var result = await handler.HandleAsync(request);

        // Assert - Demonstrates MiddleMan.Zero's NotFound result pattern
        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.NotFound),
            () => result.Response.ShouldBeNull(),
            () => result.Messages.ShouldNotBeEmpty(),
            () => result.Messages.First().ShouldBeOfType<NotFoundMessage>()
        );
    }

    [Fact]
    public async Task HandleAsync_ReturnsInvalidResult_WhenOrderIdIsEmpty()
    {
        // Arrange
        var repository = new OrderRepository();
        var handler = new GetOrderHandler(repository);
        var request = new GetOrderRequest { OrderId = Guid.Empty };

        // Act - ValidateAsync logs InvalidRequestMessage, MiddleMan.Zero fails fast
        var result = await handler.HandleAsync(request);

        // Assert - Demonstrates MiddleMan.Zero's validation and fail-fast behavior
        var invalidMessage = result.Messages.OfType<InvalidRequestMessage>().First();

        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Invalid),
            () => result.Response.ShouldBeNull(),
            () => invalidMessage.Message.ShouldBe("OrderId must be a valid non-empty GUID.")
        );
    }

    [Fact]
    public async Task HandleAsync_DoesNotExecuteHandler_WhenValidationFails()
    {
        // Arrange
        var repository = new OrderRepository();
        var handler = new GetOrderHandler(repository);
        var request = new GetOrderRequest { OrderId = Guid.Empty };

        // Act - MiddleMan.Zero's fail-fast validation prevents handler execution
        var result = await handler.HandleAsync(request);

        // Assert - Demonstrates fail-fast pattern: Invalid status, handler never ran
        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Invalid),
            () => result.Response.ShouldBeNull()
        );

        // Repository is never queried when validation fails
        var allOrders = await repository.GetAll();
        allOrders.ShouldBeEmpty();
    }
}
