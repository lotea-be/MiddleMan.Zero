using IceCreamTruck.Contracts;

using MiddleMan.Zero;

namespace IceCreamTruck.Handlers;

/// <summary>
/// Handles creation of new ice cream orders.
/// </summary>
public class CreateOrderHandler : HandlerBase<CreateOrderRequest>
{
    // In-memory storage for demo purposes
    private static readonly List<Order> Orders = [];

    protected override ValueTask HandleAsync(
        CreateOrderRequest message,
        HandlerContext context,
        CancellationToken cancellationToken = default)
    {
        var order = new Order
        {
            CustomerName = message.CustomerName,
            Items = message.Items,
            Status = OrderStatus.Pending
        };

        Orders.Add(order);

        return ValueTask.CompletedTask;
    }

    protected override ValueTask ValidateAsync(CreateOrderRequest request, HandlerContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
        {
            context.Log(new InvalidRequestMessage("Customer name is required."));
        }

        if (request.Items == null || request.Items.Count == 0)
        {
            context.Log(new InvalidRequestMessage("Order must contain at least one ice cream."));
        }

        return ValueTask.CompletedTask;
    }

}
