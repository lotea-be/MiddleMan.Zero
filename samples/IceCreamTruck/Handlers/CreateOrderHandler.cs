using IceCreamTruck.Contracts;

using MiddleMan.Zero;

namespace IceCreamTruck.Handlers;

/// <summary>
/// Handles creation of new ice cream orders.
/// </summary>
public class CreateOrderHandler(IOrderRepository repository) : HandlerBase<CreateOrderRequest, Guid>
{
    private readonly IOrderRepository _repository = repository;

    protected override async Task<Guid> HandleAsync(
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

        await _repository.AddAsync(order);

        return order.Id;
    }

    protected override Task ValidateAsync(CreateOrderRequest request, HandlerContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
        {
            context.Log(new InvalidRequestMessage("Customer name is required."));
        }

        if (request.Items == null || request.Items.Count == 0)
        {
            context.Log(new InvalidRequestMessage("Order must contain at least one ice cream."));
        }

        return Task.CompletedTask;
    }

}