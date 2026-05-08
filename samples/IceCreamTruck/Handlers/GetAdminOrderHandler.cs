using IceCreamTruck.Contracts;
using IceCreamTruck.Repositories;

using MiddleMan.Zero;

namespace IceCreamTruck.Handlers;

/// <summary>
/// Handles admin retrieval of ice cream orders by ID.
/// Demonstrates the Forbidden status on a generic handler (HandlerBase&lt;TRequest, TResponse&gt;):
/// non-admin callers are denied access, producing ResultStatus.Forbidden with a null response.
/// </summary>
public class GetAdminOrderHandler(IOrderRepository orderRepository) : HandlerBase<GetAdminOrderRequest, Order>
{
    protected override async Task<Order?> HandleAsync(
        GetAdminOrderRequest message,
        HandlerContext context,
        CancellationToken cancellationToken = default)
    {
        if (!message.IsAdminUser)
        {
            context.Log(new ForbiddenMessage());
            return null;
        }

        var order = await orderRepository.GetAsync(message.OrderId, cancellationToken);

        if (order == null)
        {
            context.Log(new NotFoundMessage());
            return null;
        }

        return order;
    }

    protected override Task ValidateAsync(GetAdminOrderRequest request, HandlerContext context, CancellationToken cancellationToken = default)
    {
        if (request.OrderId == Guid.Empty)
        {
            context.Log(new InvalidRequestMessage("OrderId must be a valid non-empty GUID."));
        }

        return Task.CompletedTask;
    }
}
