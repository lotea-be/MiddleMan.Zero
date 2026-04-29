using IceCreamTruck.Contracts;

using Microsoft.Extensions.DependencyInjection;

using MiddleMan.Zero;
using MiddleMan.Zero.Abstractions;

namespace IceCreamTruck.Tests.Handlers;

/// <summary>
/// Tests demonstrating MiddleMan.Zero features with GetAdminOrderHandler.
/// These tests focus on the Forbidden status on a GENERIC handler
/// (HandlerBase&lt;TRequest, TResponse&gt;), which exercises the fixed CreateResult overload
/// that previously lacked the Forbidden check.
/// </summary>
public class GetAdminOrderHandlerTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IOrderRepository _repository;

    public GetAdminOrderHandlerTests()
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

        var handler = _serviceProvider.GetRequiredService<IHandleAsync<GetAdminOrderRequest, Order?>>();
        var request = new GetAdminOrderRequest { OrderId = order.Id, IsAdminUser = false };

        // Act - Exercises HandlerBase<TRequest, TResponse>.CreateResult with Forbidden check
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Forbidden),
            () => result.Response.ShouldBeNull(),
            () => result.Messages.OfType<ForbiddenMessage>().ShouldNotBeEmpty()
        );
    }

    [Fact]
    public async Task HandleAsync_ReturnsSuccessfulResult_WhenAdminRetrievesExistingOrder()
    {
        // Arrange
        var order = new Order
        {
            CustomerName = "Jane Smith",
            Items = [new IceCream { Flavor = "Chocolate", Price = 6.50m, Scoops = 2 }],
            Status = OrderStatus.Pending
        };
        await _repository.AddAsync(order, TestContext.Current.CancellationToken);

        var handler = _serviceProvider.GetRequiredService<IHandleAsync<GetAdminOrderRequest, Order?>>();
        var request = new GetAdminOrderRequest { OrderId = order.Id, IsAdminUser = true };

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Successful),
            () => result.Response.ShouldNotBeNull(),
            () => result.Response!.Id.ShouldBe(order.Id),
            () => result.Response!.CustomerName.ShouldBe("Jane Smith")
        );
    }

    [Fact]
    public async Task HandleAsync_ReturnsNotFoundResult_WhenAdminRetrievesNonExistentOrder()
    {
        // Arrange
        var handler = _serviceProvider.GetRequiredService<IHandleAsync<GetAdminOrderRequest, Order?>>();
        var request = new GetAdminOrderRequest { OrderId = Guid.NewGuid(), IsAdminUser = true };

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
        var handler = _serviceProvider.GetRequiredService<IHandleAsync<GetAdminOrderRequest, Order?>>();
        var request = new GetAdminOrderRequest { OrderId = Guid.Empty, IsAdminUser = true };

        // Act
        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        // Assert
        var invalidMessage = result.Messages.OfType<InvalidRequestMessage>().First();

        result.ShouldSatisfyAllConditions(
            () => result.ResultStatus.ShouldBe(ResultStatus.Invalid),
            () => result.Response.ShouldBeNull(),
            () => invalidMessage.Message.ShouldContain("OrderId must be a valid non-empty GUID")
        );
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}
