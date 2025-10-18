using IceCreamTruck.Contracts;
using IceCreamTruck.Repositories;

using MiddleMan.Zero;

namespace IceCreamTruck.Handlers;

/// <summary>
/// Handles retrieval of ice cream orders by ID.
/// </summary>
public class GetOrderHandler(OrderRepository orderRepository) : HandlerBase<GetOrderRequest, Order>
{
    protected override async ValueTask<Order?> HandleAsync(
        GetOrderRequest message,
        HandlerContext context,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetById(message.OrderId);

        if (order == null)
        {
            context.Log(new NotFoundMessage());
            return null;
        }

        return order;
    }

    protected override ValueTask ValidateAsync(GetOrderRequest request, HandlerContext context, CancellationToken cancellationToken = default)
    {
        if (request.OrderId == Guid.Empty)
        {
            context.Log(new InvalidRequestMessage("OrderId must be a valid non-empty GUID."));
        }

        return ValueTask.CompletedTask;
    }
}
