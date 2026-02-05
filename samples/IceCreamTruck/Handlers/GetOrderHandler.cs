using IceCreamTruck.Contracts;

using MiddleMan.Zero;

namespace IceCreamTruck.Handlers;

/// <summary>
/// Handles retrieval of ice cream orders by ID.
/// </summary>
public class GetOrderHandler(IOrderRepository orderRepository) : HandlerBase<GetOrderRequest, Order>
{
    protected override async Task<Order?> HandleAsync(
        GetOrderRequest message,
        HandlerContext context,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetAsync(message.OrderId);

        if (order == null)
        {
            context.Log(new NotFoundMessage());
            return null;
        }

        return order;
    }

    protected override Task ValidateAsync(GetOrderRequest request, HandlerContext context, CancellationToken cancellationToken = default)
    {
        if (request.OrderId == Guid.Empty)
        {
            context.Log(new InvalidRequestMessage("OrderId must be a valid non-empty GUID."));
        }

        return Task.CompletedTask;
    }
}