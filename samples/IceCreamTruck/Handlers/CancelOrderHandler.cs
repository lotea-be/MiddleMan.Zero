using IceCreamTruck.Contracts;

using MiddleMan.Zero;

namespace IceCreamTruck.Handlers;

/// <summary>
/// Handles cancellation of ice cream orders.
/// Demonstrates the Forbidden status: only admin users may cancel orders.
/// </summary>
public class CancelOrderHandler(IOrderRepository repository) : HandlerBase<CancelOrderRequest>
{
    protected override async Task HandleAsync(
        CancelOrderRequest message,
        HandlerContext context,
        CancellationToken cancellationToken = default)
    {
        if (!message.IsAdminUser)
        {
            context.Log(new ForbiddenMessage());
            return;
        }

        var found = await repository.CancelAsync(message.OrderId, cancellationToken);

        if (!found)
        {
            context.Log(new NotFoundMessage());
        }
    }

    protected override Task ValidateAsync(CancelOrderRequest request, HandlerContext context, CancellationToken cancellationToken = default)
    {
        if (request.OrderId == Guid.Empty)
        {
            context.Log(new InvalidRequestMessage("OrderId must be a valid non-empty GUID."));
        }

        return Task.CompletedTask;
    }
}
